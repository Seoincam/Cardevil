using Cardevil.Card.InStage;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Attributes;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Enemy;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Cardevil.Gameplay.Field;

namespace Cardevil.Gameplay.Turn
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

        private StageCardManager _card;
        private ITurnPlayer _player;

        // 1. Field를 저장할 변수 추가
        private Field.Field _field;

        private EnemySpawner _enemySpawner;
        private ITurnEnemy _currentEnemy;

        private RerollPresenter Reroll => _card.Reroll;
        private StageCardCorePresenter CardCore => _card.Core;

        public TurnManager(StageCardManager cardManager, ITurnPlayer player, EnemySpawner enemySpawner,Field.Field field)
        {
            _card = cardManager;
            _enemySpawner = enemySpawner;
            _player = player;
            _field = field;

            ExecEventBus<PlayerMoveArgs>.RegisterStatic(0, player.OnMoveAsync);

            //  적 생성 시 Init 호출
            if (enemySpawner.TrySpawn(out var enemy))
            {
                enemy.Init(_field); // <-- 여기서 Field 전달
                _currentEnemy = enemy;
            }
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
            await Reroll.WaitUntilRerollEndAsync();

            var firstEnemyContext = new EnemyContext
            {
                PlayerPosition = _player.Position
            };
            // Enemy의 공격 범위 띄우기
            await _currentEnemy.OnEnemyCreateFirstAttackAsync(firstEnemyContext);

            while (!cancellationToken.IsCancellationRequested)
            {
                turnOrder++;
                
                await _currentEnemy.OnStartTurnAsync();
                
                await CardCore.WaitUntilPlayerInputAsync();

                // 각 카드 연산
                await CardCore.AddStepAsync(ScoreStepType.EachCard);

                // 합 연산
                await CardCore.AddStepAsync(ScoreStepType.PlusRelic);
                await CardCore.AddStepAsync(ScoreStepType.PlusField);
                await CardCore.AddStepAsync(ScoreStepType.PlusPlayerStatus);
                float score0 = await CardCore.ApplyScoreOperatorsAsync();

                // 곱 연산, 골드 연산
                await CardCore.AddStepAsync(ScoreStepType.MultiplyCardFinalDamage);
                await CardCore.AddStepAsync(ScoreStepType.MultiplyRelic);
                await CardCore.StepGoldRelicAsync();
                await CardCore.AddStepAsync(ScoreStepType.MultiplyField);
                await CardCore.AddStepAsync(ScoreStepType.MultiplyPlayerStatus);
                float score1 = await CardCore.ApplyScoreOperatorsAsync();
                // TODO: 최종 데미지 출력하기 (근데 이게 뭔지 체크해야함)
                
                await CardCore.DiscardSelectionAsync();
                
                // 적 기믹 연산
                await CardCore.AddStepAsync(ScoreStepType.EnemyStatus);
                float finalScore = score0 + score1 + await CardCore.ApplyScoreOperatorsAsync();
                // TODO: 최종 데미지 출력하기 (근데 이게 뭔지 체크해야함)
                
                // TODO: 임시로 고쳐놓음.
                
                await _player.AttackAsync(finalScore);
                await _currentEnemy.OnTakeDamageAsync(finalScore);

                var enemyContext = new EnemyContext
                {
                    PlayerPosition = _player.Position
                };

                if (_currentEnemy.IsDead)
                {
                    await _currentEnemy.OnDieAsync();

                    if (!_enemySpawner.TrySpawn(out var newEnemy))
                    {
                        // TODO: 스테이지  보상
                        break;
                    }

                    newEnemy.Init(_field);
                    _currentEnemy = newEnemy;
                   
                    await _currentEnemy.OnReplaceAsync();

                    await _currentEnemy.OnEnemyCreateFirstAttackAsync(enemyContext);
                }
                
                // TODO: 플레이어 카드로 게임 오버 체크
                
              

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

                await _currentEnemy.UpdateAttackAsync(enemyContext);

                await CardCore.DrawAsync();
            }
        }
    }
}