using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class DelayedHandRankScoreDefinition : ScoreEffectDefinition
    {
        [Header("(지연) 족보 보너스 설정")]
        [SerializeField] private HandRank targetHandRank;

        public override string EditorName => "점수/(지연) 족보 보너스";
        public override string EditorDescription => $"족보가 <color=#FFD700>{targetHandRank}</color>일 경우, <b>다음 턴</b>에 {CommonDescription}";
        
        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new RunTime(this, context);

        [Serializable]
        public class RunTime : ScoreEffectRuntime
        {
            private readonly DelayedHandRankScoreDefinition _definition;
            
            private bool _executeNext;

            [Serializable]
            public class State
            {
                public bool executeNext;
            }
            
            public RunTime(DelayedHandRankScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override object CaptureState()
            {
                var state = new State { executeNext = _executeNext };
                return JsonUtility.ToJson(state);
            }

            public override void RestoreState(object stateObj)
            {
                if (stateObj is string json)
                {
                    var state = JsonUtility.FromJson<State>(json);
                    if (state != null)
                    { 
                        _executeNext = state.executeNext;
                    }
                }
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                ScoreOperator scoreOperator = null; 
                
                if (_executeNext)
                {
                    float finalValue = Definition.GetCalculatedValue(Context);
                    scoreOperator = new ScoreOperator
                    {
                        Type = Definition.OperatorType, Value = finalValue, Source = this
                    };
                    _executeNext = false;
                }
                
                if (context.HandRankData.HandRank == _definition.targetHandRank)
                {
                    _executeNext = true;
                }

                return scoreOperator;
            }
        }
    }
}