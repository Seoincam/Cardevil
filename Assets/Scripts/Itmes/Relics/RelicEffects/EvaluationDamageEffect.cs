using Cardevil.Cards;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Relic
{
    public enum TriggerType { Ranking, Health, Always, }
    public enum ExecuteType { Immediate, Next, Permanent }

    [CreateAssetMenu(menuName = "Relics/Relic Effect/Evaluation Damage")]
    public class EvaluationDamageEffect : RelicEffectSO
    {
        [Header("Trigger")]
        [SerializeField] TriggerType triggerType;
        [SerializeField] HandRanking triggerRanking = HandRanking.None;
        [SerializeField] int triggerInt;

        [Header("Effect")]
        [SerializeField] CardResultEvaluator.EffectType effectType;
        [SerializeField] int effectPlus;
        [SerializeField] float effectMultiply;

        [Header("Execute")]
        [SerializeField] ExecuteType executeType = ExecuteType.Immediate; 
        [SerializeField, Range(0, 1)] float possibility = 1;
        [SerializeField] bool onlyOnPushToHistory = false;

        [Header("Execute Options")]
        [SerializeField] int executionCount;
        [SerializeField] List<HandRanking> targetRankings = new();



        public override void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (this.effectType != effectType)
                return;

            switch (triggerType)
            {
                case TriggerType.Ranking: ApplyBasedRanking(effectType, resultCtx, evaluationCtx); break;
            }
        }

        private void ApplyBasedRanking(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            if (executeType == ExecuteType.Immediate)
            {
                if (evaluationCtx.Ranking != triggerRanking)
                    return;

                switch (effectType)
                {
                    case CardResultEvaluator.EffectType.Plus:
                        evaluationCtx.rankingDamage += effectPlus;
                        break;

                    case CardResultEvaluator.EffectType.Multiply:
                        evaluationCtx.defaultDamage *= effectMultiply;
                        evaluationCtx.rankingDamage *= effectMultiply;
                        break;
                }
            }

            else if (executeType == ExecuteType.Next)
            {
                if (!targetRankings.Contains(evaluationCtx.Ranking))
                    return;

                int count = 0;

                // 역순으로(최신부터) History에 접근
                for (int i = 1; i <= resultCtx.History.Count; i++)
                {
                    if (resultCtx.History[^i] is not { } r)
                        continue;

                    if (r.Ranking == triggerRanking)
                    {
                        // 역순으로 접근 중 trigger ranking 만났을 때
                        // count에 따라 실행 여부를 판단함.
                        if (count < executionCount)
                        {
                            switch (effectType)
                            {
                                case CardResultEvaluator.EffectType.Plus:
                                    evaluationCtx.rankingDamage += effectPlus;
                                    break;

                                case CardResultEvaluator.EffectType.Multiply:
                                    evaluationCtx.defaultDamage *= effectMultiply;
                                    evaluationCtx.rankingDamage *= effectMultiply;
                                    break;
                            }
                        }

                        break;
                    }

                    if (targetRankings.Contains(r.Ranking))
                    {
                        count++;
                        continue;
                    }
                }
            }

            else if (executeType == ExecuteType.Permanent)
            {
                if (!targetRankings.Contains(evaluationCtx.Ranking))
                    return;
                    
                switch (effectType)
                {
                    case CardResultEvaluator.EffectType.Plus:
                        evaluationCtx.rankingDamage += effectPlus;
                        break;

                    case CardResultEvaluator.EffectType.Multiply:
                        evaluationCtx.defaultDamage *= effectMultiply;
                        evaluationCtx.rankingDamage *= effectMultiply;
                        break;
                }
            }
        }
    }
}