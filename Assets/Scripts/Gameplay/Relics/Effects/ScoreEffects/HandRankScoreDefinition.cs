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


        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private readonly HandRankScoreDefinition _definition;
            
            public Runtime(HandRankScoreDefinition definition, RelicInstance context) : base(definition, context)
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