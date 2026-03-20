using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class RepeatedHandRankScoreDefinition : ScoreEffectDefinition
    {
        public override string EditorName => "점수/연속 족보 보너스";
        public override string EditorDescription => $"연속된 족보 사용시, {CommonDescription}";
        
        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private HandRank _previousHandRank = HandRank.None;
            
            public Runtime(ScoreEffectDefinition definition, RelicInstance context) : base(definition, context)
            {
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                IScoreOperator scoreOperator = null;

                if (context.HandRank != HandRank.None && context.HandRank == _previousHandRank)
                {
                    scoreOperator = new ScoreOperator
                    {
                        Type = Definition.OperatorType,
                        Value = Definition.GetCalculatedValue(Context),
                        Source = this
                    };
                }

                _previousHandRank = context.HandRankData.HandRank;
                return scoreOperator;
            }
        }
    }
}