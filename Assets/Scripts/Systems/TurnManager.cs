using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Cardevil.Core;
using Cardevil.Utils;

namespace Cardevil.Systems
{
    public class TurnManager: IClearable
    {
        public int TurnOrder { get; private set; }
        
        private CancellationTokenSource _cts;
        private UniTask _loopTask;

        private ITurnCardFlow _cardFlow;
        private ITurnPlayer _player;
        private ITurnEnemy _enemy;


        public void Clear()
        {
            StopLoopAsync().Forget();
            TurnOrder = 0;
            _cardFlow = null;
            _player = null;
            _enemy = null;
        }
        
        public void Init(ITurnCardFlow cardFlow, ITurnPlayer player, ITurnEnemy enemy)
        {
            Clear();

            _cardFlow = cardFlow ?? throw new ArgumentNullException(nameof(cardFlow));
            _player = player ?? throw new ArgumentNullException(nameof(player));
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

                await _cardFlow.EnterRerollPhase(6);
                await _cardFlow.Reroll.Reroll();
                await _cardFlow.ExitRerollPhase();

                await _cardFlow.EnterHandPhase();
                while (!token.IsCancellationRequested)
                {
                    TurnOrder++;
                    await _cardFlow.StageCards.DrawCard();
                    if (_cardFlow.StageCards.IsNoCard)
                    {
                        // TODO: 게임 오버
                        break;
                    }

                    await _cardFlow.StageCards.WaitUserInput();

                    await _player.TurnMove();
                    await _player.TurnAttack();

                    if (_enemy.IsDead)
                    {
                        // TODO: 스테이지 클리어
                        break;
                    }

                    await _enemy.TurnAttack();

                    if (_player.IsDead)
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
            }
            finally
            {
                await _cardFlow.ExitHandPhase();
            }
        }
    }
}
