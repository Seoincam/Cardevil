using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    // 특정 족보일 시, 추가 점수(데미지)
    [CreateAssetMenu(menuName = "Relics/Relic Effect/Random Damage Bonus")]
    public class RandomDamageBonusEffectSO : RelicEffectSO
    {
        [SerializeField] CardResultEvaluator.EffectType _effectType;
        [SerializeField, Tooltip("실제 사용할 때만 적용되나?")] bool onlyOnPushToHistory;
        [SerializeField, Range(0, 1)] float _possibility;
        [SerializeField] float _bonusDamageMultiply;

        public float Possibility => _possibility;
        public float BonusDamageMultiply => _bonusDamageMultiply;

        public override void Apply()
        {
            Debug.LogWarning("유물의 잘못된 사용.");
        }

        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (onlyOnPushToHistory && !evaluationCtx.pushToHistory)
                return;
                
            if (_effectType == effectType && Random.value < _possibility)
            {
                // TODO: 실제론 사용시에만 계산.
                evaluationCtx.defaultDamage = Mathf.Round(evaluationCtx.defaultDamage *= _bonusDamageMultiply);
                evaluationCtx.rankingDamage = Mathf.Round(evaluationCtx.rankingDamage *= _bonusDamageMultiply);
            }
        }
    }
}