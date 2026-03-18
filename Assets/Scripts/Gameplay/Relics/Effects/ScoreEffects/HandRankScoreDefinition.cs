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
        public override string EditorDescription => $"족보가 <color=#FFD700>{targetHandRank}</color>일 경우, <color=#FFD700>{Value}점({OperatorType})</color>을 부여합니다.";


        public override EffectRuntime CreateRuntimeInstance(IRelicContext context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private readonly HandRankScoreDefinition _definition;
            
            public Runtime(ScoreEffectDefinition definition, IRelicContext context) : base(definition, context)
            {
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (context.HandRankData.HandRank != _definition.targetHandRank)
                {
                    return null;
                }

                return new ScoreOperator { Type = Definition.OperatorType, Value = Definition.Value, Source = this };
            }
        }
    }
}