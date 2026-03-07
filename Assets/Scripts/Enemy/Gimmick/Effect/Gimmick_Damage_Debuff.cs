using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.InGame.Enemy
{
    /// <summary>

    /// </summary>
    public class Gimmick_Damage_Debuff : IGimmick, IScoreProvider
    {
        private Enemy _targetEnemy;

        private int _scoreProviderId = -1;

        public ScoreStepType ScoreStepType => ScoreStepType.EnemyStatus;


        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;
            _scoreProviderId = CardevilCore.Instance.Game.ScoreProviderRegistry.Register(this);
            // TODO: View 등록하기
        }
        
        public IScoreOperator GetScoreOperator(IScoreContext context)
        {
            float damageCap = _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];
            
            if (context.CurrentScore > damageCap)
            {
                var originalDamage = context.CurrentScore;
                Debug.Log($"[Gimmick] 데미지 제한 발동: {originalDamage} -> {damageCap} (최대치: {damageCap})");
                
                return new ScoreOperator
                {
                    Source = this,
                    Type = ScoreOperatorType.Plus,
                    Value = originalDamage - damageCap
                };
            }

            return null;
        }

        public void Remove()
        {
            CardevilCore.Instance.Game.ScoreProviderRegistry.SafeUnregister(_scoreProviderId, this);
            _scoreProviderId = -1;
        }
    }
}

