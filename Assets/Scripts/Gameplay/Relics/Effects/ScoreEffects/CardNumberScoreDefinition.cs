using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class CardNumberScoreDefinition : ScoreEffectDefinition
    {
        [Header("카드 숫자 보너스 설정")]
        [SerializeField] private List<int> targetNumbers;

        public override string EditorName => "점수/카드 숫자 보너스";
        public override string EditorDescription => $"카드의 <color=#FFD700>숫자가 {string.Join(", ", targetNumbers)}</color>일 경우, {CommonDescription}";

        public override EffectInstance CreateRuntimeInstance(RelicInstance context) => new Instance(this, context);

        [Serializable]
        public class Instance : ScoreEffectInstance
        {
            private readonly CardNumberScoreDefinition _definition;
                
            public Instance(CardNumberScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                var currentNumber = context.CurrentCard.Numbers.Current!.Value;
                if (!_definition.targetNumbers.Contains(currentNumber))
                {
                    return null;
                }

                float finalValue = Definition.GetCalculatedValue(Context);
                return new ScoreOperator { Type = Definition.OperatorType, Value = finalValue, Source = this };
            }
        }
    }
}