using Cardevil.Attributes;
using Cardevil.Cards.InStage.Model.ReadOnly;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageOnEachCardEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        // "적을 처치할수록, 카드의 기본 데미지가 영구히 +1 증가합니다" 플래그
        [SerializeField, VisibleOnly] private bool isBasedKillCount;
        [SerializeField, VisibleOnly] private int damage;

        public bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel) => true;
        
        public DamageOnEachCardEffect(string effectId, bool isBasedKillCount, int damage)
        {
            this.effectId = effectId;
            this.isBasedKillCount = isBasedKillCount;
            this.damage = damage;
        }

        
    }
}