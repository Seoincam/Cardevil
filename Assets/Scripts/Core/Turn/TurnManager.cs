using Cardevil.Attributes;
using Cardevil.Enemy;
using Cardevil.Events.ExecEvents;
using Cardevil.NewCard.InStage;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Cardevil.Core.Turn
{
    public interface IEnemyContext
    {
        TileVector PlayerPosition { get; }
    }
    
    public class EnemyContext : IEnemyContext
    {
        public TileVector PlayerPosition { get; set; }
    }
    
    [Serializable]
    public class TurnManager : IDisposable
    {
        [SerializeField, VisibleOnly] private int turnOrder;
        
        private StageCardCorePresenter _cardCore;
        private ITurnPlayer _player;

        private EnemySpawner _enemySpawner;
        private ITurnEnemy _currentEnemy;

        public TurnManager(StageCardCorePresenter cardCore, ITurnPlayer player, EnemySpawner enemySpawner)
        {
            _cardCore = cardCore;
            _enemySpawner = enemySpawner;
            _player = player;
            
            ExecEventBus<PlayerMoveArgs>.RegisterStatic(0, player.OnMoveAsync);
            
            // 임시 
            enemySpawner.TrySpawn(out var enemy);
            _currentEnemy = enemy;
        }
        
        public void Dispose()
        {
            ExecEventBus<PlayerMoveArgs>.UnregisterStatic(_player.OnMoveAsync);
        }

        public void StartLoop()
        {
            // 입장 전 실행할 로직들
            
            CoreLoopAsync().Forget();
        }
        
        private async UniTask CoreLoopAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                turnOrder++;
                
                await _currentEnemy.OnStartTurnAsync();
                
                await _cardCore.WaitUntilPlayerInputAsync();

                // 합 연산
                await _cardCore.StepEachCardAsync();
                await _cardCore.StepPlusRelicAsync();
                await _cardCore.StepPlusFieldAsync();
                await _cardCore.StepPlusPlayerStatusAsync();
                await _cardCore.ApplyScoreOperatorsAsync();

                // 곱 연산, 골드 연산
                await _cardCore.StepMultiplyCardFinalDamageAsync();
                await _cardCore.StepMultiplyRelicAsync();
                await _cardCore.StepGoldRelicAsync();
                await _cardCore.StepMultiplyFieldAsync();
                await _cardCore.StepMultiplyPlayerStatusAsync();
                float score = await _cardCore.ApplyScoreOperatorsAsync();
                
                // TODO: 최종 데미지 출력하기 (근데 이게 뭔지 체크해야함)
                
                await _cardCore.DiscardSelectionAsync();
                
                // 적 기믹 연산
                await _cardCore.StepEnemyStatusAsync();
                float finalScore = await _cardCore.ApplyScoreOperatorsAsync();
                
                // TODO: 최종 데미지 출력하기 (근데 이게 뭔지 체크해야함)
                
                await _player.AttackAsync(finalScore);
                await _currentEnemy.OnTakeDamageAsync(finalScore);

                if (_currentEnemy.IsDead)
                {
                    await _currentEnemy.OnDieAsync();

                    if (!_enemySpawner.TrySpawn(out var newEnemy))
                    {
                        // TODO: 스테이지  보상
                        break;
                    }

                    _currentEnemy = newEnemy;
                    await _currentEnemy.OnReplaceAsync();
                }
                
                // TODO: 플레이어 카드로 게임 오버 체크
                
                var enemyContext = new EnemyContext
                {
                    PlayerPosition = _player.Position
                };

                bool enemyShouldAttack = await _currentEnemy.CheckAttackCountAsync();
                if (enemyShouldAttack)
                {
                    var (attackSuccess, damage) = await _currentEnemy.TryAttackAsync(enemyContext);
                    if (attackSuccess)
                    {
                        await _player.TakeDamageAsync(damage);
                        
                        if (_player.IsDead)
                        {
                            // TODO: 게임 오버
                            break;
                        }

                        await _currentEnemy.OnAttackSuccessAsync();
                    }
                }
                
                await _currentEnemy.OnEndTurnAsync();
                // TODO: 플레이어, 유물

                if (enemyShouldAttack)
                {
                    await _currentEnemy.UpdateAttackAsync(enemyContext);
                }

                await _cardCore.DrawAsync();
            }
        }
    }
}