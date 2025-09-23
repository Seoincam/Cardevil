using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    // 특정 족보일 시, 다음에 추가 점수(데미지)
    [CreateAssetMenu(menuName = "Relics/Relic Effect/Ranking Based Next Damage")]
    public class RankingBasedNextDamageEffectSO : RelicEffectSO
    {
        [SerializeField] CardResultEvaluator.EffectType _effectType;
        [SerializeField, Tooltip("효과 발동 족보")] HandRanking _triggerRanking;
        [SerializeField, Tooltip("최소 적용 족보")] HandRanking _targetMinimumRanking;
        [SerializeField] float _bonusDamageMultiply;

        public HandRanking TriggerRanking => _triggerRanking;
        public HandRanking TargetMinimumRanking => _targetMinimumRanking;
        public float BonusDamageMultiply => _bonusDamageMultiply;

        public override void Apply()
        {
            Debug.LogWarning("유물의 잘못된 사용.");
        }

        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (_effectType == effectType && evaluationCtx.Ranking >= _targetMinimumRanking)
            {
                // 역순으로(최신부터) History에 접근
                for (int i = 1; i <= resultCtx.History.Count; i++)
                {
                    if (resultCtx.History[^i] is not { } r)
                    {
                        // null이면 건너뜀
                        continue;
                    }

                    if (r.Ranking > _triggerRanking)
                    {
                        // trigger ranking보다 높은 게 먼저 나온다면
                        // 효과 적용할 필요 없음
                        break;
                    }

                    if (r.Ranking == _triggerRanking)
                    {
                        // 역순으로 접근 중 trigger ranking 만났다면
                        // evaluation Ctx(현재 계산)에 효과 적용함
                        evaluationCtx.defaultDamage = Mathf.Round(evaluationCtx.defaultDamage *= _bonusDamageMultiply);
                        evaluationCtx.rankingDamage = Mathf.Round(evaluationCtx.rankingDamage *= _bonusDamageMultiply);
                        break;
                    }
                }
            }
        }
    }
}