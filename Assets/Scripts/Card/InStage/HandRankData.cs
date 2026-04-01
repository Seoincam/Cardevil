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
        [field: SerializeField] public CardColor CardColor { get; private set; }
        
        public IReadOnlyList<ICardState> RankedCards { get; }
        
        public static HandRankData None = new(HandRank.None);

        public HandRankData(
            HandRank handRank,
            IReadOnlyList<ICardState> rankedCards,
            CardColor cardColor = CardColor.None)
        {
            HandRank = handRank;
            CardColor = cardColor;
            RankedCards = rankedCards;
        }

        public HandRankData(HandRank handRank)
        {
            HandRank = handRank;
            CardColor = CardColor.None;
            RankedCards = Array.Empty<ICardState>();
        }
    }
}