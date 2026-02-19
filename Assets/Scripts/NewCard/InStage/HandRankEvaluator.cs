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
            
            if (attacks.Length == 0) return new HandRankData(HandRank.None);

            foreach (var card in attacks)
            {
                if (!card.ValueSelected) return new HandRankData(HandRank.None);
            }

            Debug.Assert(attacks.All(card => card.Colors.Current.HasValue));
            Debug.Assert(attacks.All(card => card.Numbers.Current.HasValue));

            var info = new HandInfo(attacks);

            if (info.IsFlush && info.IsFourCard)
                return new HandRankData(HandRank.FourCardFlush, info.All, info.Color);

            if (info.IsFlush && info.IsStraight)
                return new HandRankData(HandRank.StraightFlush, info.All, info.Color);

            if (info.IsFlush && info.PairCount == 2)
                return new HandRankData(HandRank.TwoPairFlush, info.All, info.Color);

            if (info.IsFourCard)
                return new HandRankData(HandRank.FourCard, info.All);

            if (info.IsStraight)
                return new HandRankData(HandRank.Straight, info.All);

            if (info.IsTriple)
            {
                var cards = info.Groups.First(g => g.Count() >= 3).ToArray();
                return new HandRankData(HandRank.Triple, cards);
            }

            if (info.PairCount == 2)
            {
                var cards = info.Groups.Where(g => g.Count() == 2).SelectMany(g => g).ToArray();
                return new HandRankData(HandRank.TwoPair, cards);
            }

            if (info.PairCount == 1)
            {
                var cards = info.Groups.First(g => g.Count() == 2).ToArray();
                return new HandRankData(HandRank.OnePair, cards);
            }

            var highCard = info.All
                .OrderByDescending(c => c.Numbers.Current!.Value)
                .First();
            return new HandRankData(HandRank.HighCard, new[] { highCard });
        }
        
        private readonly struct HandInfo
        {
            public bool IsFlush { get; }
            public bool IsStraight { get; }
            public bool IsFourCard { get; }
            public bool IsTriple { get; }
            public int PairCount { get; }
            public CardColor Color { get; }

            public IReadOnlyList<IGrouping<int, ICardState>> Groups { get; }
            public IReadOnlyList<ICardState> All { get; }

            public HandInfo(IReadOnlyList<ICardState> attacks)
            {
                All = attacks;
                int count = attacks.Count;

                var firstColor = attacks[0].Colors.Current!.Value;
                Color = firstColor;
                IsFlush = count == 4 &&
                          attacks.All(c => c.Colors.Current!.Value == firstColor);

                Groups = attacks
                    .GroupBy(c => c.Numbers.Current!.Value)
                    .ToList();

                IsFourCard = Groups.Any(g => g.Count() >= 4);
                IsTriple = Groups.Any(g => g.Count() >= 3);
                PairCount = Groups.Count(g => g.Count() == 2);

                if (count == 4 && Groups.Count == 4)
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