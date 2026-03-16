using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    [Serializable]
    public class RandomBonusEffect : ScoreEffectBase
    {
        [SerializeField, Range(0f, 1f)] private float probability;

        public RandomBonusEffect() { }
        public RandomBonusEffect(
            IRelicContext context, 
            ScoreStepType scoreStepType, 
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id, 
            float probability) 
            : base(context, scoreStepType, scoreOperatorType, value, id)
        {
            this.probability = probability;
        }

        protected override IScoreOperator InternalGetScoreOperator(IScoreContext context)
        {
            if (RandomUtil.GetRandomFloat(0f, 1f) < probability)
            {
                return null;
            }
            
            return new ScoreOperator
            {
                Type = scoreOperatorType,
                Value = value,
                Source = this
            };
        }
    }
}