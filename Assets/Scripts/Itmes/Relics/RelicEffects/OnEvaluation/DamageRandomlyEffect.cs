using Cardevil.Attributes;
using Cardevil.Cards.InStage.Model.ReadOnly;
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

        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
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