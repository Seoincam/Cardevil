using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    // 특정 Health일 시, 추가 데미지
    [CreateAssetMenu(menuName = "Relics/Relic Effect/Health Based Damage")]
    public class HealthBasedDamageEffectSO : RelicEffectSO
    {
        [SerializeField] CardResultEvaluator.EffectType _effectType;
        [SerializeField] int _targetHealth;
        [SerializeField] float _bonusDamageMultiply;

        public int TargetHealth => _targetHealth;
        public float BonusDamageMultiply => _bonusDamageMultiply;

        public override void Apply()
        {
            Debug.LogWarning("유물의 잘못된 사용.");
        }

        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (_effectType == effectType && Managers.Game.PlayerStatus.CurrentHp == _targetHealth)
            {
                evaluationCtx.defaultDamage = Mathf.Round(evaluationCtx.defaultDamage * _bonusDamageMultiply);
                evaluationCtx.rankingDamage   = Mathf.Round(evaluationCtx.rankingDamage * _bonusDamageMultiply);
            }
        }
    }
}