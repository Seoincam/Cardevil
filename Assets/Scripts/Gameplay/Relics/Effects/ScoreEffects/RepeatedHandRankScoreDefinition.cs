using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class RepeatedHandRankScoreDefinition : ScoreEffectDefinition
    {
        public override string EditorName => "점수/연속 족보 보너스";
        public override string EditorDescription => $"연속된 족보 사용시, {CommonDescription}";
        
        public override EffectInstance CreateRuntimeInstance(RelicInstance context) => new Instance(this, context);

        [Serializable]
        public class Instance : ScoreEffectInstance
        {
            private HandRank _previousHandRank = HandRank.None;

            [Serializable]
            public class State
            {
                public HandRank previousHandRank;
            }

            public Instance(ScoreEffectDefinition definition, RelicInstance context) : base(definition, context)
            {
            }

            public override object CaptureState()
            {
                var state = new State { previousHandRank = _previousHandRank };
                return state;
            }

            public override void RestoreState(object stateObj)
            {
                if (stateObj is string json)
                {
                    var state = JsonUtility.FromJson<State>(json);
                    if (state != null)
                    {
                        _previousHandRank = state.previousHandRank;
                    }
                }
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