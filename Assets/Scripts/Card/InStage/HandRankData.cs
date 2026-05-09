using Cardevil.Card.Common.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    [Serializable]
    public struct HandRankData
    {
        [field: SerializeField] public HandRank HandRank { get; private set; }
        
        /// <summary>
        /// 플러시 계열일 경우 대표색도 저장함.
        /// </summary>
        [field: SerializeField] public CardColor FlushColor { get; private set; }
        
        public IReadOnlyList<INewCardState> RankedCards { get; }
        
        public static HandRankData None = new(HandRank.None);

        public HandRankData(
            HandRank handRank,
            IReadOnlyList<INewCardState> rankedCards,
            CardColor flushColor = CardColor.None)
        {
            HandRank = handRank;
            FlushColor = flushColor;
            RankedCards = rankedCards;
        }

        public HandRankData(HandRank handRank)
        {
            HandRank = handRank;
            FlushColor = CardColor.None;
            RankedCards = Array.Empty<INewCardState>();
        }
    }
}