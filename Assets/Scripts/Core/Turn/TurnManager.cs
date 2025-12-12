using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn.Interfaces;
using Cardevil.Enemy;
using Cardevil.Utils;

namespace Cardevil.Core.Turn
{
    public class TurnManager: IClearable
    {
        private CancellationTokenSource _cts;
        private UniTask _loopTask;
        
        /*
         * 추후 동시에 여러 적과
         * 싸우는 등의 변화가 있다면,
         * target을 지정하는 식으로
         * 쉽게 전환할 수 있음.
         */
        
        private ITurnCardFlow _cardFlow;
        private ITurnPlayer _player;
        private ITurnEnemy _enemy;

        private TurnContext _ctx;
        private EnemySpawner _spawner;


        public void Clear()
        {
            StopLoopAsync().Forget();
            _cardFlow = null;
            _enemy = null;
            _player = null;
        }

        public void Init(EnemySpawner spawner, ITurnCardFlow cardFlow, ITurnPlayer player, ITurnEnemy enemy)
        {
            _spawner = spawner;
            _cardFlow = cardFlow;
            _player = player;
            _enemy = enemy;
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

            _ctx = new TurnContext();
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
                LogEx.Log("Game Loop");
                
                // TODO: 적에 대한 설명 표시
                await _enemy.ShowInitialAttackArea();

                await _cardFlow.EnterRerollPhase(6);
                await _cardFlow.Reroll();
                await _cardFlow.ExitRerollPhase();
                
                await _cardFlow.EnterHandPhase();
                
                while (!token.IsCancellationRequested)
                {
                    _ctx.TurnCount++;
                    await _cardFlow.DrawCard();
                    if (_cardFlow.IsNoCard)
                    {
                        // TODO: 게임 오버
                        break;
                    }

                    await _cardFlow.WaitUserInput();
                    
                    // 2. 플레이어 이동 + 공격
                    var playerPosition = await _player.TurnMove();
                    _ctx.PlayerPosition = playerPosition;
                    
                    var playerAttack = await _enemy.TurnAttackAsync(_ctx);
                    playerAttack.Target.TakeDamage(playerAttack.Damage);

                    if (_enemy.IsDead)
                    {
                        if (!_spawner.TrySpawn(out var spawned))
                            break;

                        await _enemy.Replace();
                        _enemy = spawned;
                    }

                    // 3. 적 공격
                    var enemyAttack = await _enemy.TurnAttackAsync(_ctx);
                    enemyAttack.Target.TakeDamage(enemyAttack.Damage);
                    
                    if (_player.IsDead)
                    {
                        Bootstrapper.Instance.Game.PlayerDied();
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
