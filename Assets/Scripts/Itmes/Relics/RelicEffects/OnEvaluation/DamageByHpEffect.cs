using Cardevil.Attributes;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageByHpEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [Header("ByHp")]
        [SerializeField, VisibleOnly] private int triggerHp;
        
        [Space, SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private int damageAmount;
        [SerializeField, VisibleOnly] private float damageMultiplier;
        
        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel) 
            => Managers.Game.PlayerStatus.CurrentHp == triggerHp;

        public DamageByHpEffect(string effectId, int triggerHp, bool isPlus, float value)
        {
            this.effectId = effectId;
            
            this.triggerHp = triggerHp;
            
            this.isPlus = isPlus;
            if (isPlus) damageAmount = (int)value;
            else damageMultiplier = value;
        }
    }
}