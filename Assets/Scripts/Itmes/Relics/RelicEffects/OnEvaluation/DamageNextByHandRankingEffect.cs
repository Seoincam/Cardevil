using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageNextByHandRankingEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [Header("Next By HandRanking")]
        [SerializeField, VisibleOnly] private HandRanking triggerHandRanking;
        [SerializeField, VisibleOnly] private bool isPermanent;
        [SerializeField, VisibleOnly] private int executionCount;
        
        [Space, SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private int damageAmount;
        [SerializeField, VisibleOnly] private float damageMultiplier;
        
        public bool IsPlus => isPlus;
        
        public bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel)
        {
            var h = resultModel.History;
            if (h.Count == 0)
                return false;
            
            // 최근부터 과거로 스캔하는 방식으로 체크
            int remaining = executionCount;
            for (int i = h.Count - 1; i >= 0; i--)
            {
                var result = h[i];
                if (result == null || result.HandRanking == HandRanking.None)
                    continue;

                if (result.HandRanking == triggerHandRanking)
                    return isPermanent || remaining > 0;

                if (!isPermanent && --remaining <= 0)
                    return false;
            }
            
            return false;
        }

        public EvaluationStep MakeEvaluationStep()
        {
            var type = isPlus ? EvaluationStep.Type.Plus : EvaluationStep.Type.Multiply;
            var value = isPlus ? damageAmount : damageMultiplier;
            
            return EvaluationStep.Get()
                .SetValue(type, value);
        }

        public DamageNextByHandRankingEffect(string effectId, HandRanking triggerHandRanking, bool isPermanent,
            int executionCount, bool isPlus, float value)
        {
            this.effectId = effectId;
            
            this.triggerHandRanking = triggerHandRanking;
            this.isPermanent = isPermanent;
            this.executionCount = executionCount;
            
            this.isPlus = isPlus;
            if (isPlus) damageAmount = (int)value;
            else damageMultiplier = value;
        }
    }
}