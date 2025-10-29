using Cardevil.Attributes;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageRandomlyEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [SerializeField, VisibleOnly] private float triggerPossibility; // 0f~1f
        [SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private float damage;

        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
        }
        
        public DamageRandomlyEffect(string effectId, float triggerPossibility, bool isPlus, float damage)
        {
            this.effectId = effectId;
            
            this.triggerPossibility = triggerPossibility;
            this.isPlus = isPlus;
            this.damage = damage;
        }
    }
}