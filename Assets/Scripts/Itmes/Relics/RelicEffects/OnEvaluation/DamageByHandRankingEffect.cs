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
        [SerializeField, VisibleOnly] private HandRanking triggerHandRanking;
        [SerializeField, VisibleOnly] private bool isPlus;
        [SerializeField, VisibleOnly] private float damage;
        
        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel)
        {
            throw new System.NotImplementedException();
        }

        public DamageByHandRankingEffect(string effectId, 
            HandRanking triggerHandRanking, bool isPlus, float damage)
        {
            this.effectId = effectId;
            
            this.triggerHandRanking = triggerHandRanking;
            this.isPlus = isPlus;
            this.damage = damage;
        }
    }
}