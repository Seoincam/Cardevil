using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    // 하이카드 공격이 다음 공격의 최종 데미지를 125% 증가시킵니다.
    [Serializable]
    public class HandRankNextBonusEffect : ScoreEffectBase
    {
        [Header("HandRank Next Bonus")]
        [SerializeField] private HandRank targetHandRank;
        
        private bool needToExecute;
        
        public HandRankNextBonusEffect() { }
        public HandRankNextBonusEffect(
            IRelicContext context, 
            ScoreStepType scoreStepType,
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id) 
            : base(context, scoreStepType, scoreOperatorType,
            value, id)
        {
        }

        protected override IScoreOperator InternalGetScoreOperator(IScoreContext context)
        {
            ScoreOperator scoreOperator = null;
            
            if (needToExecute)
            {
                needToExecute = false;
                
                scoreOperator = new ScoreOperator
                {
                    Type = scoreOperatorType,
                    Value = value,
                    Source = this
                };
            }

            if (context.HandRankData.HandRank == targetHandRank)
            {
                needToExecute = true;
            }

            return scoreOperator;
        }
        }
    }
