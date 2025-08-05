using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Utils
{
    public static class CardComboEvaluator
    {
        public static CardResult Evaluate(IEnumerable<Card> cardDatas)
        {
            var moveCards = cardDatas.Select(c => c.cardData)
                .Where(c => c.type == CardType.Move)
                .Select(c => c.direction)
                .ToArray();

            var numberCards = cardDatas.Select(c => c.cardData)
                .Where(c => c.type == CardType.Number)
                // .Select(c => c.type == CardType.Number)
                .ToList();

            var baseDamage = numberCards.Sum(c => c.value);
            var combo = CalculateCardCombo(numberCards);
            var comboDamage = (int)combo;

            return new CardResult(baseDamage, combo, moveCards);
        }

        #region 카드 족보 판정

        private static CardCombo CalculateCardCombo(List<CardData> cards)
        {
            if (IsStraightFlush(cards))
                return CardCombo.StraightFlush;
            else if (IsFourCard(cards))
                return CardCombo.FourCard;
            else if (IsStraight(cards))
                return CardCombo.Straight;
            else if (IsFlush(cards))
                return CardCombo.Flush;
            else if (IsTriple(cards))
                return CardCombo.Triple;
            else if (IsTwoPair(cards))
                return CardCombo.TwoPair;
            else if (IsOnePair(cards))
                return CardCombo.OnePair;

            return CardCombo.High;
        }

        private static bool IsStraight(List<CardData> cards)
        {
            if (cards.Count != 4)
                return false;

            var values = cards.Select(c => c.value)
                .OrderBy(v => v)
                .ToList();

            for (int i = 1; i < cards.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;

            return true;
        }

        private static bool IsFlush(List<CardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.Select(c => c.color)
                .Distinct()
                .Count() == 1;
        }

        private static bool IsStraightFlush(List<CardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return IsStraight(cards) && IsFlush(cards);
        }

        static bool IsFourCard(List<CardData> cards)
        {
            return cards.GroupBy(c => c.value)
                        .Any(g => g.Count() == 4);
        }

        static bool IsTriple(List<CardData> cards)
        {
            return cards.GroupBy(c => c.value)
                .Any(g => g.Count() == 3);
        }

        static bool IsTwoPair(List<CardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.GroupBy(c => c.value)
                    .Where(g => g.Count() == 2)
                    .Count() == 2;
        }

        static bool IsOnePair(List<CardData> cards)
        {
            return cards.GroupBy(c => c.value)
                        .Any(g => g.Count() == 2);
        }

        #endregion
    }
}