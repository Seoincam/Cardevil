using Cardevil.NewCard.Common.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    public static class HandRankEvaluator
    {
        public static HandRankData GetHandRank(IReadOnlyList<ICardState> selection)
        {
            if (selection == null || selection.Count == 0) return new HandRankData(HandRank.None);
            
            var attacks = selection
                .Where(c => c.IsAttack)
                .ToArray();
            
            foreach (var card in attacks)
            {
                if (!card.ValueSelected) return new HandRankData(HandRank.None);
            }
            
            Debug.Assert(attacks.All(card => card.Colors.Current.HasValue), "One or more cards' colors are null.");
            Debug.Assert(attacks.All(card => card.Numbers.Current.HasValue), "One or more cards' numbers are null.");

            var info = new HandInfo(attacks);

            if (info.IsFlush && info.IsFourCard)
                return new HandRankData(HandRank.FourCardFlush, info.Color);

            if (info.IsFlush && info.IsStraight)
                return new HandRankData(HandRank.StraightFlush, info.Color);

            if (info.IsFlush && info.PairCount == 2)
                return new HandRankData(HandRank.TwoPairFlush, info.Color);

            if (info.IsFourCard)
                return new HandRankData(HandRank.FourCard);

            if (info.IsStraight)
                return new HandRankData(HandRank.Straight);

            if (info.IsTriple)
                return new HandRankData(HandRank.Triple);

            if (info.PairCount == 2)
                return new HandRankData(HandRank.TwoPair);

            if (info.PairCount == 1)
                return new HandRankData(HandRank.OnePair);

            return new HandRankData(HandRank.HighCard);
        }
        
        private readonly struct HandInfo
        {
            public bool IsFlush { get; }
            public bool IsStraight { get; }
            public bool IsFourCard { get; }
            public bool IsTriple { get; }
            public int PairCount { get; }
            public CardColor Color { get; }

            public HandInfo(IReadOnlyList<ICardState> attacks)
            {
                int count = attacks.Count;

                // flush
                var firstColor = attacks[0].Colors.Current!.Value;
                Color = firstColor;
                IsFlush = count == 4 &&
                          attacks.All(c => c.Colors.Current!.Value == firstColor);

                // 숫자 그룹
                var groups = attacks
                    .GroupBy(c => c.Numbers.Current!.Value)
                    .Select(g => g.Count())
                    .ToList();

                IsFourCard = groups.Any(c => c >= 4);
                IsTriple = groups.Any(c => c >= 3);
                PairCount = groups.Count(c => c == 2);

                // straight
                if (count == 4 && groups.Count == 4)
                {
                    var numbers = attacks.Select(c => c.Numbers.Current!.Value);
                    IsStraight = numbers.Max() - numbers.Min() == 3;
                }
                else
                {
                    IsStraight = false;
                }
            }
        }
    }
}