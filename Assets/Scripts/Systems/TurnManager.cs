using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Cardevil.Core;
using Cardevil.Utils;

namespace Cardevil.Systems
{
    public class TurnManager: IClearable
    {   
        private CancellationTokenSource _cts;
        private UniTask _loopTask;

        private ITurnPlayerInput _playerInput;
        private ITurnRerollInput _rerollInput;
        private ITurnPlayerMove _playerMove;
        private ITurnPlayerAction _playerAction;
        private ITurnEnemy _enemy;


        public void Clear()
        {
            StopLoopAsync().Forget();
            _playerInput = null;
            _rerollInput = null;
            _playerMove = null;
            _playerAction = null;
            _enemy = null;
        }
        
        public void Init(ITurnRerollInput rerollInput, ITurnPlayerInput playerInput, ITurnPlayerMove playerMove, ITurnPlayerAction playerAction, ITurnEnemy enemy)
        {
            Clear();
            
            _rerollInput = rerollInput ?? throw new ArgumentNullException(nameof(rerollInput));
            _playerInput = playerInput ?? throw new ArgumentNullException(nameof(playerInput));
            _playerMove = playerMove ?? throw new ArgumentNullException(nameof(playerMove));
            _playerAction = playerAction ?? throw new ArgumentNullException(nameof(playerAction));
            _enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
        }


        /// <summary>
        /// 스테이지 입장 시 턴 루프 시작. 외부 토큰과 링크.
        /// </summary>
        public void StartLoop(CancellationToken external = default)
        {
            _ = StartLoopAsync(external);
        }

        private async UniTask StartLoopAsync(CancellationToken external = default)
        {
            // 혹시 돌고 있던 루프가 있으면 안전 종료까지 대기
            await StopLoopAsync();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(external);
            _loopTask = GameLoopAsync(_cts.Token);
        }

        /// <summary>
        /// 추후 스테이지 매니저 등 외부에서 await로 호출.
        /// </summary>
        public async UniTask StopLoopAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();
            try
            {
                LogEx.Log("Loop 정지 요청 받음.");
                await _loopTask;
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소
            }
            catch (Exception e)
            {
                LogEx.LogError($"Stop Loop error: {e}");
            }
            finally
            {
                LogEx.Log("Loop 정지 완료.");
                _cts.Dispose();
                _cts = null;
                _loopTask = default;
            }
        }


        private async UniTask GameLoopAsync(CancellationToken token)
        {
            try
            {
                // TODO: 적에 대한 설명 표시
                await _enemy.TurnAttack();
                await _rerollInput.RerollCard();

                while (!token.IsCancellationRequested)
                {
                    await _playerInput.DrawCard();
                    // TODO: 게임 오버

                    _playerInput.ActivateInteraction();
                    await _playerInput.WaitUserInput();
                    _playerInput.InactivateInteraction();

                    await _playerMove.TurnMove();
                    await _playerAction.TurnAttack();

                    if (_enemy.IsDead)
                    {
                        // TODO: 스테이지 클리어
                        break;
                    }

                    await _enemy.TurnAttack();

                    if (_playerAction.IsDead)
                    {
                        Managers.Game.PlayerDied();
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상적인 취소
            }
            catch (Exception e)
            {
                LogEx.LogError($"Loop exception: {e}");
                // throw;
            }
        }
    }
}
