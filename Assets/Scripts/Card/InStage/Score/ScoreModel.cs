using Cardevil.Card.Common.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        public int HandRankScore
        {
            get
            {
                if (HandRank == HandRank.None) return 0;
                
                var data = CardevilCore.Database.Database.HandRankDataList.FirstOrDefault(x => x.Ranking == handRankData.HandRank);
                if (data == null)
                {
                    LogEx.LogError($"DB에서 족보 데이터를 찾지 못했습니다: {HandRank}");
                    return 0;
                }

                return data.Value;
            }
        }
        
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