using Cardevil.Card.Common.Core;
using Cardevil.Core.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;
using Random = UnityEngine.Random;

namespace Cardevil.Card.InStage.Score
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
        public float HandRankScore => HandRank == HandRank.None ? 0f : Random.Range(0, 100f);
        
        public void SetScore(float value)
        {
            score = value;
        }
        
        public void SetHandRank(in HandRankData handRank)
        {
            handRankData = handRank;
            SetScore(HandRankScore);
        }

        public void AddOperator(IScoreOperator scoreOperator)
        {
            scoreOperators.Add(scoreOperator);
        }

        public void ClearOperators()
        {
            scoreOperators.Clear();
        }
        
        public void Clear()
        {
            score = 0;
            handRankData = HandRankData.None;
            scoreOperators.Clear();
        }
    }
}