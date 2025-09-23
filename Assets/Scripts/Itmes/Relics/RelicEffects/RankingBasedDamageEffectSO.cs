using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    // 특정 족보일 시, 추가 점수(데미지)
    [CreateAssetMenu(menuName = "Relics/Relic Effect/Ranking Bonus Damage")]
    public class RankingBasedDamageEffectSO : RelicEffectSO
    {
        [SerializeField] CardResultEvaluator.EffectType _effectType;
        [SerializeField] HandRanking _targetRanking;
        [SerializeField] int _bonusDamage;

        public HandRanking TargetRanking => _targetRanking;
        public int BonusDamage => _bonusDamage;

        public override void Apply()
        {
            Debug.LogWarning("유물의 잘못된 사용.");
        }

        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (_effectType == effectType && evaluationCtx.Ranking == _targetRanking)
            {
                evaluationCtx.rankingDamage += _bonusDamage;
            }

        }
    }
}