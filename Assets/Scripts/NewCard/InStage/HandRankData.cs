using Cardevil.NewCard.Common.Core;
using System;
using System.Collections.Generic;

namespace Cardevil.NewCard.InStage
{
    public readonly struct HandRankData
    {
        public HandRank HandRank { get; }
        
        /// <summary>
        /// 플러시 계열일 경우 대표색도 저장함.
        /// </summary>
        public CardColor CardColor { get; }
        public IReadOnlyList<ICardState> RankedCards { get; }

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