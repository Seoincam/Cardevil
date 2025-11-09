using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageByHandRankingEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [Header("By HandRanking")]
        [SerializeField, VisibleOnly] private HandRanking triggerHandRanking;
        
        [Space, SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private int damageAmount;
        [SerializeField, VisibleOnly] private float damageMultiplier;

        public bool IsPlus => isPlus;
        
        public bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel) =>
            currentHandRanking == triggerHandRanking;

        public EvaluationStep MakeEvaluationStep()
        {
            var type = isPlus ? EvaluationStep.Type.Plus : EvaluationStep.Type.Multiply;
            var value = isPlus ? damageAmount : damageMultiplier;
            
            return EvaluationStep.Get()
                .SetValue(type, value);
        }

        public DamageByHandRankingEffect(string effectId, 
            HandRanking triggerHandRanking, bool isPlus, float value)
        {
            this.effectId = effectId;
            
            this.triggerHandRanking = triggerHandRanking;
            
            this.isPlus = isPlus;
            if (isPlus) damageAmount = (int)value;
            else damageMultiplier = value;
        }
    }
}