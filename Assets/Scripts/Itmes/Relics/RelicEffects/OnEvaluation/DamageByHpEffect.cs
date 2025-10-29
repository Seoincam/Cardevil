using Cardevil.Attributes;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageByHpEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [SerializeField, VisibleOnly] private int triggerHp;
        [SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private float damage;
        
        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
        }

        public DamageByHpEffect(string effectId, int triggerHp, bool isPlus, float damage)
        {
            this.effectId = effectId;
            
            this.triggerHp = triggerHp;
            this.isPlus = isPlus;
            this.damage = damage;
        }
    }
}