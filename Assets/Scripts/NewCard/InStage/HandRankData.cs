using Cardevil.NewCard.Common.Core;

namespace Cardevil.NewCard.InStage
{
    public readonly struct HandRankData
    {
        public HandRank HandRank { get; }
        
        /// <summary>
        /// 플러시 계열일 경우 대표색도 저장함.
        /// </summary>
        public CardColor CardColor { get; }

        public HandRankData(HandRank handRank, CardColor cardColor = CardColor.None)
        {
            HandRank = handRank;
            CardColor = cardColor;
        }
    }
}