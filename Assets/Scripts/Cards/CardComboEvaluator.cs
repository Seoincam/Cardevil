using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Cards
{
    public static class CardComboEvaluator
    {
        private static bool hasDamageRelics = false;
        private static bool hasColorRelics = false;
        
        public static CardResult Evaluate(IEnumerable<Card> cards)
        {
            var moveCards = cards.Select(c => c.data)
                        .OfType<DirectionCard>()
                        .Select(d => d.DefaultValue)
                        .ToArray();

            var numberCards = cards.Select(c => c.data)
                        .OfType<NumberCard>()
                        .ToList();

            // == 숫자 카드 판정 ==
            if (numberCards.Count == 0)
                return new CardResult(moveCards);


            // == damage 합 연산 유물 ==
            if (hasDamageRelics)
            {
                // damage = damage + 기본 공격 데미지 * 사용된 카드 수
            }


            // == 족보 판정 ==
            if (hasColorRelics)
            {
                // 해당 유물의 [같은색취급 세트]에서, 색의 개수가 같거나 더 적은 색을 나머지 색으로 변환시킨다.
            }

            var combo = CalculateCombo(numberCards);
            var damage = 0;

            if (combo == CardCombo.High)
            {
                damage = numberCards.OrderBy(c => c.DefaultValue).Last().DefaultValue;
            }
            else
            {
                if (combo == CardCombo.OnePair)
                {
                    damage = 2 * numberCards.GroupBy(c => c.DefaultValue)
                        .Where(g => g.Count() == 2)
                        .Sum(g => g.Key);
                }
                else if (combo == CardCombo.Triple)
                {
                    damage = 3 * numberCards.GroupBy(c => c.DefaultValue)
                        .Where(g => g.Count() == 3)
                        .Sum(g => g.Key);
                }
                else
                    damage = numberCards.Sum(c => c.DefaultValue);
            }

            damage += (int)combo;

            // == 데미지 곱 연산 ==

            // == 데미지 강화 카드의 존재 여부 ==

            // == 유물 데미지 판정 ==


            return new CardResult(combo, damage, moveCards);
        }



        #region 카드 족보 판정

        private static CardCombo CalculateCombo(List<NumberCard> cards)
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

        private static bool IsStraight(List<NumberCard> cards)
        {
            if (cards.Count != 4)
                return false;

            var values = cards.Select(c => c.DefaultValue)
                    .OrderBy(v => v)
                    .ToList();

            for (int i = 1; i < cards.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;

            return true;
        }

        private static bool IsFlush(List<NumberCard> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.Select(c => c.Color)
                    .Distinct()
                    .Count() == 1;
        }

        private static bool IsStraightFlush(List<NumberCard> cards)
        {
            if (cards.Count != 4)
                return false;

            return IsStraight(cards) && IsFlush(cards);
        }

        static bool IsFourCard(List<NumberCard> cards)
        {
            return cards.GroupBy(c => c.DefaultValue)
                        .Any(g => g.Count() == 4);
        }

        static bool IsTriple(List<NumberCard> cards)
        {
            return cards.GroupBy(c => c.DefaultValue)
                        .Any(g => g.Count() == 3);
        }

        static bool IsTwoPair(List<NumberCard> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.GroupBy(c => c.DefaultValue)
                    .Where(g => g.Count() == 2)
                    .Count() == 2;
        }

        static bool IsOnePair(List<NumberCard> cards)
        {
            return cards.GroupBy(c => c.DefaultValue)
                        .Any(g => g.Count() == 2);
        }

        #endregion
    }
}