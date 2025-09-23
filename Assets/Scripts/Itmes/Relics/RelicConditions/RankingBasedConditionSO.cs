using Cardevil.Cards;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.Relic
{
    [CreateAssetMenu(menuName = "Relics/Relic Condition/Ranking Based")]
    public class RankingBasedConditionSO : RelicConditionSO
    {
        [Header("Trigger")]
        [SerializeField] HandRanking triggerRanking;

        [Header("Execute")]
        [SerializeField] ExecuteType executeType = ExecuteType.Immediate;
        [SerializeField, Range(0, 1)] float possibility = 1;
        [SerializeField] bool onlyOnUse = false;

        [Header("(Next/Permanent Options)")]
        [SerializeField] int executionCount = 0;
        [SerializeField] List<HandRanking> targetRankings = new();



        public override bool CanTrigger(CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (Random.value > possibility)
                return false;
            if (onlyOnUse && !evaluationCtx.pushToHistory)
                return false;

            return executeType switch
            {
                ExecuteType.Immediate => CanImmediateTrigger(evaluationCtx),
                ExecuteType.Next => CanNextTrigger(resultCtx, evaluationCtx),
                ExecuteType.Permanent => CanPermanentTrigger(resultCtx, evaluationCtx),
                _ => false
            };

        }



        private bool CanImmediateTrigger(CardEvaluationContext evaluationCtx)
        {
            return triggerRanking == evaluationCtx.Ranking;
        }

        private bool CanNextTrigger(CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (!targetRankings.Contains(evaluationCtx.Ranking))
                return false;
                
            int count = 0;
            bool canTrigger = false;

            // 역순으로(최신부터) History에 접근
            for (int i = resultCtx.History.Count - 1; i >= 0; --i)
            {
                if (resultCtx.History[i] is not { } r)
                    continue;

                if (r.Ranking == triggerRanking)
                {
                    // 역순으로 접근 중 trigger ranking 만났을 때
                    // count에 따라 실행 여부를 판단함.
                    if (count < executionCount)
                    {
                        canTrigger = true;
                        break;
                    }
                    break;
                }

                if (targetRankings.Contains(r.Ranking))
                {
                    count++;
                    continue;
                }
            }

            return canTrigger;
        }

        private bool CanPermanentTrigger(CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (!targetRankings.Contains(evaluationCtx.Ranking))
                return false;

            bool canTrigger = false;
            // 역순으로(최신부터) History에 접근
            for (int i = resultCtx.History.Count - 1; i >= 0; --i)
            {
                if (resultCtx.History[^i] is not { } r)
                    continue;

                if (r.Ranking == triggerRanking)
                {
                    // 역순으로 접근 중 trigger ranking 만났다면 true
                    canTrigger = true;
                    break;
                }
            }

            return canTrigger;
        }

    }
}