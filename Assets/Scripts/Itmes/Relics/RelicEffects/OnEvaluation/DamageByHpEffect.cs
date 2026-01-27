using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Cards.Evaluation;
using Cardevil.Cards.InStage;
using Cardevil.Core.Bootstrap;
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
        
        public bool IsPlus => isPlus;
        
        public bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel)
            => CardevilCore.Instance.Game.PlayerStatus.CurrentHp == triggerHp;

        public EvaluationStep MakeEvaluationStep()
        {
            var type = isPlus ? EvaluationStep.Type.Plus : EvaluationStep.Type.Multiply;
            var value = isPlus ? damageAmount : damageMultiplier;
            
            return EvaluationStep.Get()
                .SetValue(type, value);
        }

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