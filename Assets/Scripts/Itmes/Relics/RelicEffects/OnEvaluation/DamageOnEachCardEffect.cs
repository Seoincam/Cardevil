using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Cards.Evaluation;
using Cardevil.Cards.InStage;
using System;
using UnityEngine;

namespace Cardevil.Relics.OnEvaluation
{
    [Serializable]
    public class DamageOnEachCardEffect : RelicEffectBase, IRelicEffectOnEvaluation
    {
        [Header("On Each Card")]
        [SerializeField, VisibleOnly] private bool isBasedKillCount; // "적을 처치할수록, 카드의 기본 데미지가 영구히 +1 증가합니다" 플래그
        [SerializeField, VisibleOnly] private int damage;
        
        public bool IsPlus => true;
        
        public bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel)
            => true;

        public EvaluationStep MakeEvaluationStep()
        {
            var type = EvaluationStep.Type.Plus;
            var value = isBasedKillCount ? damage : damage; // TODO: 적 처치 수 어딘가에 저장해야함.
            
            return EvaluationStep.Get()
                .SetValue(type, value);
        }

        public DamageOnEachCardEffect(string effectId, bool isBasedKillCount, int damage)
        {
            this.effectId = effectId;
            this.isBasedKillCount = isBasedKillCount;
            this.damage = damage;
        }
    }
}