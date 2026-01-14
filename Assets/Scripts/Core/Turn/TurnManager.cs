using Cardevil.Attributes;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using Cardevil.Core.Turn.Interfaces;
using Cardevil.Enemy;
using Cardevil.Utils;
using UnityEngine;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.Core.Turn
{
    [Serializable]
    public class TurnManager: IClearable
    {
        [field: SerializeField, VisibleOnly] public int TurnOrder { get; private set; }
        
        private CancellationTokenSource _cts;
        private UniTask _loopTask;
        
        private ITurnCardFlow _cardFlow;
        private ITurnTarget2 _targetEnemy;
        private ITurnTarget2 _targetPlayer; 
        
        private EnemySpawner _spawner;

        public interface ITurnTarget2
        {
            bool IsDead { get; }
        }
            
        public void Clear()
        {
            StopLoopAsync().Forget();
            _cardFlow = null;
        }

        public void Init(EnemySpawner spawner, ITurnCardFlow cardFlow)
        {
            _spawner = spawner;
            _cardFlow = cardFlow;
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

            var enterStageArgs = EnterStageArgs.Get(TileVector.Zero, 6);
            await ExecEventBus<EnterStageArgs>.InvokeMergedAndDispose(enterStageArgs, _cts.Token);

            // await _enemy.ShowInitialAttackArea(initialPlayerPosition);
            //
            // await _cardFlow.EnterRerollPhase(6);
            // await _cardFlow.Reroll();
            // await _cardFlow.ExitRerollPhase();
            //     
            // await _cardFlow.EnterHandPhase();
            
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
        
        private async UniTask GameLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    TurnOrder++;
                    
                    await _cardFlow.DrawCard();
                    if (_cardFlow.IsNoCard)
                    {
                        // TODO: 게임 오버
                        break;
                    }

                    await _cardFlow.WaitUserInput();
                    
                    // 모든 카드 사용.
                    // 플레이어는 이때 이동을 함.
                    await _cardFlow.UseAllCardsAsync(cancellationToken);
                    
                    // 공격 카드 데미지 계산.
                    // EvaluationResult에 모든 계산이 끝난 데미지, 플레이어 위치, 족보가 모두 포함되어 있음.
                    // 이 시점에선 result는 불변임.
                    var evaluationResult = await _cardFlow.EvaluateAsync(cancellationToken);
                    
                    // 플레이어의 공격 + 적의 타격
                    var playerAttackArgs = PlayerAttackArgs2.Get(evaluationResult);
                    await ExecEventBus<PlayerAttackArgs2>.InvokeMergedAndDispose(playerAttackArgs, cancellationToken);
                    
                    if (_targetEnemy.IsDead)
                    {
                        // if (!_spawner.TrySpawn(out var spawned))
                        //     break;
                        // // TODO: Enemy에 필드를 주입해줘야함.
                        // await _enemy.Replace();
                        // _enemy = spawned;
                    }

                    // 3. 적의 공격 + 플레이어의 타격
                    var enemyAttackArgs = EnemyAttackArgs.Get();
                    await ExecEventBus<EnemyAttackArgs>.InvokeMergedAndDispose(enemyAttackArgs, cancellationToken);

                    if (_targetPlayer.IsDead)
                    {
                        
                    }
                    

                    // Enemy Turn이 끝났을때 전파
                    await ExecEventBus<EnemyTurnEndArgs>.InvokeMergedAndDispose(EnemyTurnEndArgs.Get(), cancellationToken);
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
                // TurnLoopEnded?.Invoke(context);
                // await _cardFlow.ExitHandPhase();
            }
        }
    }
}
