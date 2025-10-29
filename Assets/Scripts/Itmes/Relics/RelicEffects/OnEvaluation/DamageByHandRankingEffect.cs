using Cardevil.Attributes;
using Cardevil.Cards.Data;
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
        
        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
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