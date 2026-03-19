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
        public override string EditorDescription => $"카드의 <color=#FFD700>숫자가 {targetNumber}</color>일 경우, {CommonDescription}";

        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private readonly CardNumberScoreDefinition _definition;
                
            public Runtime(CardNumberScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (context.CurrentCard.Numbers.Current!.Value != _definition.targetNumber)
                {
                    return null;
                }

                float finalValue = Definition.GetCalculatedValue(Context);
                return new ScoreOperator { Type = Definition.OperatorType, Value = finalValue, Source = this };
            }
        }
    }
}