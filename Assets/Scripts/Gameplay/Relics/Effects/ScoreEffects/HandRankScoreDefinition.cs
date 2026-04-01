using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class HandRankScoreDefinition : ScoreEffectDefinition
    {
        [Header("족보 보너스 설정")]
        [SerializeField] private HandRank targetHandRank;

        public override string EditorName => "점수/족보 보너스";
        public override string EditorDescription => $"족보가 <color=#FFD700>{targetHandRank}</color>일 경우, {CommonDescription}";


        public override EffectInstance CreateRuntimeInstance(RelicInstance context) => new Instance(this, context);

        [Serializable]
        public class Instance : ScoreEffectInstance
        {
            private readonly HandRankScoreDefinition _definition;
            
            public Instance(HandRankScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (context.HandRankData.HandRank != _definition.targetHandRank)
                {
                    return null;
                }

                float finalValue = Definition.GetCalculatedValue(Context);
                return new ScoreOperator { Type = Definition.OperatorType, Value = finalValue, Source = this };
            }
        }
    }
}