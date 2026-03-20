using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class StarCardScoreDefinition : ScoreEffectDefinition
    {
        public override string EditorName => "점수/오망성 카드 보너스";
        public override string EditorDescription => $"오망성 카드일 경우, {CommonDescription}";
        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);

        public class Runtime : ScoreEffectRuntime
        {
            public Runtime(StarCardScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (!context.CurrentCard.IsStar) return null;

                float finalValue = Definition.GetCalculatedValue(Context);
                return new ScoreOperator { Type = Definition.OperatorType, Value = finalValue, Source = this };
            }
        }
    }
}