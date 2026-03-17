using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    [Serializable]
    public class EachCardBonusEffect : ScoreEffectBase
    {
        [Header("Each Card Bonus")]
        [SerializeField] private int targetNumber;

        public EachCardBonusEffect() { }
        public EachCardBonusEffect(
            IRelicContext context, 
            ScoreStepType scoreStepType, 
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id) 
            : base(context, scoreStepType, scoreOperatorType, value, id)
        {
        }

        protected override IScoreOperator InternalGetScoreOperator(IScoreContext context)
        {
            if (context.CurrentCard.Numbers.Current!.Value != targetNumber)
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