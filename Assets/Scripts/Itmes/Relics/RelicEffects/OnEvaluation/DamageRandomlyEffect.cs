using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Utils;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageRandomlyEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [Header("Randomly")]
        [SerializeField, VisibleOnly] private float triggerPossibility; // 0f~1f
        
        [Space, SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private int damageAmount;
        [SerializeField, VisibleOnly] private float damageMultiplier;
        
        public bool IsPlus => isPlus;
        
        public bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel)
            => RandomUtil.GetValue() < triggerPossibility;

        public EvaluationStep MakeEvaluationStep()
        {
            var type = isPlus ? EvaluationStep.Type.Plus : EvaluationStep.Type.Multiply;
            var value = isPlus ? damageAmount : damageMultiplier;
            
            return EvaluationStep.Get()
                .SetValue(type, value);
        }

        public DamageRandomlyEffect(string effectId, float triggerPossibility, bool isPlus, float value)
        {
            this.effectId = effectId;
            
            this.triggerPossibility = triggerPossibility;
            
            this.isPlus = isPlus;
            if (isPlus) damageAmount = (int)value;
            else damageMultiplier = value;
        }
    }
}