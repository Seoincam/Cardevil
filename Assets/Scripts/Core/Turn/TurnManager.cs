using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Cards.InStage;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
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
        private ITurnTarget _targetEnemy;
        private ITurnTarget _targetPlayer; 
        
        private EnemySpawner _spawner;

        public interface ITurnTarget
        {
            bool IsDead { get; }
        }
            
        public void Clear()
        {
            StopLoopAsync().Forget();
            _cardFlow = null;
        }

        public void Initialize(EnemySpawner spawner, ITurnCardFlow cardFlow, ITurnTarget player, ITurnTarget enemy)
        {
            _spawner = spawner;
            _cardFlow = cardFlow;
        }
        

        /// <summary>
        /// 스테이지 입장 시 턴 루프 시작. 외부 토큰과 링크.
        /// </summary>
        public async UniTask EnterLoopAsync(CancellationToken external = default)
        {
            // 혹시 돌고 있던 루프가 있으면 안전 종료까지 대기
            await StopLoopAsync();
            
            _cts = CancellationTokenSource.CreateLinkedTokenSource(external);

            // 입장 시 이벤트
            var enterStageArgs = EnterStageArgs.Get(TileVector.Zero, 6);
            await ExecEventBus<EnterStageArgs>.InvokeMergedAndDispose(enterStageArgs, _cts.Token);
            
            // 코어 루프 시작
            _loopTask = CoreLoopAsync(_cts.Token);
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
        
        // TODO: 손패를 다쓰면 게임 오버 - @seoincam
        private async UniTask CoreLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TurnOrder++;
                await ExecEventBus<PlayerTurnStartArgs>.InvokeMergedAndDispose(PlayerTurnStartArgs.Get(), cancellationToken);
                
                await _cardFlow.WaitPlayerInputCompleted(cancellationToken);
                await _cardFlow.UseAllCardsAsync(cancellationToken);
                var evaluationResult = await _cardFlow.EvaluateDamageAsync(cancellationToken);
                
                // 플레이어의 공격 + 적의 피격
                if (evaluationResult != null)
                {
                    var playerAttackArgs = PlayerAttackArgs.Get((EvaluationResult)evaluationResult);
                    await ExecEventBus<PlayerAttackArgs>.InvokeMergedAndDispose(playerAttackArgs, cancellationToken);
                }
                
                if (_targetEnemy.IsDead)
                {
                    // if (!_spawner.TrySpawn(out var spawned))
                    //     break;
                    // // TODO: Enemy에 필드를 주입해줘야함.
                    // await _enemy.Replace();
                    // _enemy = spawned;
                }

                // 3. 적의 공격 + 플레이어의 피격
                var enemyAttackArgs = EnemyAttackArgs.Get();
                await ExecEventBus<EnemyAttackArgs>.InvokeMergedAndDispose(enemyAttackArgs, cancellationToken);

                if (_targetPlayer.IsDead)
                {
                    
                }
                

                // Enemy Turn이 끝났을때 전파
                await ExecEventBus<EnemyTurnEndArgs>.InvokeMergedAndDispose(EnemyTurnEndArgs.Get(), cancellationToken);
            }
        }
        
    }
}
