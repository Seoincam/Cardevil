using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class RandomChanceScoreDefinition : ScoreEffectDefinition
    {
        [Header("랜덤 확률 보너스 설정")]
        [SerializeField, Range(0f, 1f)] private float probability;

        public override string EditorName => "점수/랜덤 확률 보너스";
        public override string EditorDescription => $"<color=#FFD700>{probability * 100:f2}% 확률</color>로 {CommonDescription}";


        public override EffectRuntime CreateRuntimeInstance(RelicInstance context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private readonly RandomChanceScoreDefinition _definition;
            
            public Runtime(RandomChanceScoreDefinition definition, RelicInstance context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (RandomUtil.GetRandomFloat(0f, 1f) < _definition.probability) return null;
                
                float finalValue = Definition.GetCalculatedValue(Context);
                return new ScoreOperator { Type = Definition.OperatorType, Value = finalValue, Source = this };
            }
        }
    }
}