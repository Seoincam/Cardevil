using Cardevil.Attributes;
using Cardevil.Cards.Data;
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
        
        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
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