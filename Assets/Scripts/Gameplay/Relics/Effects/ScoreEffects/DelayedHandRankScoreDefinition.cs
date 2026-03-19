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
            
            public bool executeNext;
            
            public RunTime(DelayedHandRankScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                ScoreOperator scoreOperator = null; 
                
                if (executeNext)
                {
                    scoreOperator = new ScoreOperator
                    {
                        Type = Definition.OperatorType, Value = Definition.Value, Source = this
                    };
                    executeNext = false;
                }
                
                if (context.HandRankData.HandRank == _definition.targetHandRank)
                {
                    executeNext = true;
                }

                return scoreOperator;
            }
        }
    }
}