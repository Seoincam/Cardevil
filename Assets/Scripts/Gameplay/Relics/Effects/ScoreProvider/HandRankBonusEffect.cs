using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    [Serializable]
    public class HandRankBonusEffect : ScoreEffectBase
    {
        [SerializeField] private HandRank targetHandRank;

        public HandRankBonusEffect() { }
        public HandRankBonusEffect(
            IRelicContext context, 
            ScoreStepType scoreStepType, 
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id, 
            HandRank targetHandRank) 
            : base(context, scoreStepType, scoreOperatorType, value, id)
        {
            this.targetHandRank = targetHandRank;
        }

        protected override IScoreOperator InternalGetScoreOperator(IScoreContext context)
        {
            if (context.HandRankData.HandRank != targetHandRank)
                return null;
            
            return new ScoreOperator
            {
                Type = scoreOperatorType,
                Value = value,
                Source = this
            };
        }
    }
}