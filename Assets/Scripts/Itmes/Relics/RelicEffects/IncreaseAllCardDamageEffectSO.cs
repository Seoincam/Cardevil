using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    // 모든 카드의 기본 데미지 강화
    [CreateAssetMenu(menuName = "Relics/Relic Effect/Increase All Card Damage")]
    public class IncreaseAllCardDamageEffectSO : RelicEffectSO
    {
        [SerializeField] int _damageIncreaseAmount;

        public int DamageIncreaseAmount => _damageIncreaseAmount;

        public override void Apply()
        {
            foreach (var card in Managers.Card.RuntimeBaseDeck)
            {
                card.AdditionalDamage += 10;
            }
        }

        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            Debug.LogWarning("유물의 잘못된 사용.");
        }
    }
}