using Cardevil.Attributes;
using Cardevil.NewCard.Common.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.NewCard.InStage.Score
{
    [Serializable]
    public class ScoreModel
    {
        [SerializeField, VisibleOnly] private float score;
        [SerializeField, VisibleOnly] private HandRankData handRankData;
        [SerializeReference, VisibleOnly] private List<IScoreOperator> scoreOperators = new();
        
        public float Score => score;
        public HandRank HandRank => handRankData.HandRank;
        public IReadOnlyList<IScoreOperator> ScoreOperators => scoreOperators;
        public float HandRankScore => Random.Range(0, 100f);
        
        public void SetScore(float value)
        {
            score = value;
        }
        
        public void SetHandRank(in HandRankData handRank)
        {
            handRankData = handRank;
            SetScore(HandRankScore);
        }

        public IScoreOperator AddOperator(ScoreOperatorType operatorType, float value)
        {
            var scoreOperator = new ScoreOperator
            {
                Type = operatorType, 
                Value = value
            };
            
            scoreOperators.Add(scoreOperator);
            return scoreOperator;
        }

        public void Clear()
        {
            score = 0;
            handRankData = HandRankData.None;
            scoreOperators.Clear();
        }
    }
}