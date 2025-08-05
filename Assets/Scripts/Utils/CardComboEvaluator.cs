using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Cardevil.Cards;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Utils
{
    public static class CardComboEvaluator
    {
        private static bool hasDamageRelics = false;
        private static bool hasColorRelics = false;
        
        public static CardResult Evaluate(IEnumerable<Card> cardDatas)
        {
            var moveCards = cardDatas.Select(c => c.cardData)
                        .Where(c => c.type == CardType.Move)
                        .Select(c => c.direction)
                        .ToArray();

            var numberCards = cardDatas.Select(c => c.cardData)
                        .Where(c => c.type == CardType.Number)
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
            var baseDamage = 0;
            var comboDamage = 0;

            if (combo == CardCombo.High)
            {
                baseDamage = numberCards.OrderBy(c => c.value).Last().value;
                // high 로직
            }
            else
            {
                if (combo == CardCombo.OnePair)
                {
                    baseDamage = numberCards.GroupBy(c => c.value)
                        .Where(g => g.Count() == 2)
                        .Sum(g => g.Key);
                }
                else if (combo == CardCombo.Triple)
                {
                    baseDamage = numberCards.GroupBy(c => c.value)
                        .Where(g => g.Count() == 3)
                        .Sum(g => g.Key);
                }
                else
                    baseDamage = numberCards.Sum(c => c.value);
            }

            comboDamage = (int)combo;

            var totalDamage = baseDamage + comboDamage;

            // 데미지 곱 연산

            // 데미지 강화 카드의 존재 여부

            // 유물 데미지 판정


            return new CardResult(baseDamage: 0, combo: CardCombo.None, moveCards);


            
        }

        #region 카드 족보 판정

        private static CardCombo CalculateCombo(List<CardData> cards)
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