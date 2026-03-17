using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    [Serializable]
    public class TargetStatBonusEffect : ScoreEffectBase
    {
        [Header("Target Stat Bonus")]
        [SerializeField] private PlayerStatType targetStatType;
        [SerializeField] private int targetValue;

        public TargetStatBonusEffect() { }
        public TargetStatBonusEffect(
            IRelicContext context, 
            ScoreStepType scoreStepType, 
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id, 
            PlayerStatType targetStatType, 
            int targetValue) 
            : base(context, scoreStepType, scoreOperatorType, value, id)
        {
            this.targetStatType = targetStatType;
            this.targetValue = targetValue;
        }

        protected override IScoreOperator InternalGetScoreOperator(IScoreContext context)
        {
            if (Context.PlayerStatus.GetFinalValue(targetStatType) != targetValue)
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