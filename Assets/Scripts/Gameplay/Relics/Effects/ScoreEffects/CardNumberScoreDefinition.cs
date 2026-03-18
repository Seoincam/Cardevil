using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class CardNumberScoreDefinition : ScoreEffectDefinition
    {
        [Header("카드 숫자 보너스 설정")]
        [SerializeField, Range(2f, 10f)] private int targetNumber;

        public override string EditorName => "점수/카드 숫자 보너스";
        public override string EditorDescription => $"카드의 <color=#FFD700>숫자가 {targetNumber}</color>일 경우, <color=#FFD700>{Value}점({OperatorType})</color>을 부여합니다.";

        public override EffectRuntime CreateRuntimeInstance(IRelicContext context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private readonly CardNumberScoreDefinition _definition;
                
            public Runtime(ScoreEffectDefinition definition, IRelicContext context) : base(definition, context)
            {
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (context.CurrentCard.Numbers.Current!.Value != _definition.targetNumber)
                {
                    return null;
                }

                return new ScoreOperator { Type = Definition.OperatorType, Value = Definition.Value, Source = this };
            }
        }
    }
}