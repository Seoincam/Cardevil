using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Cards
{
    public static class CardResultEvaluator
    {       
        public static void Evaluate(CardContext context, IEnumerable<Card> cards)
        {
            context.GetSet();

            var directions = cards.Select(c => c.data)
                        .OfType<DirectionCardData>()
                        .Select(d => d.value)
                        .ToList();

            var numberCards = cards.Select(c => c.data)
                        .OfType<NumberCardData>()
                        .ToList();

            var isRedCardOver3 = numberCards.Select(c => c.color == CardColor.Red)
                        .Count() >= 3;

            var isBlackCardOver3 = numberCards.Select(c => c.color == CardColor.Black)
                        .Count() >= 3;


            // == 숫자 카드 판정 ==
            if (numberCards.Count == 0)
            {
                context.SetResult(new CardResult(0, directions, null, false, false));
                return;
            } 

            // == damage 합 연산 유물 ==
            // if (hasDamageRelics)
            // {
            //     damage = damage + 기본 공격 데미지 * 사용된 카드 수
            // }

            // == 족보 판정 ==
            // if (hasColorRelics)
            // {
            //     해당 유물의 [같은색취급 세트]에서, 색의 개수가 같거나 더 적은 색을 나머지 색으로 변환시킨다.
            // }

            var rankings = CalculateRanking(numberCards);
            float damage = 0;

            if (rankings.First() == HandRanking.High)
                damage = numberCards.OrderBy(c => c.value).Last().value;

            else if (rankings.First() == HandRanking.OnePair)
                damage = numberCards.GroupBy(c => c.value)
                            .Where(g => g.Count() == 2)
                            .Sum(g => g.Key * 2);

            else if (rankings.First() == HandRanking.Triple)
                damage = numberCards.GroupBy(c => c.value)
                            .Where(g => g.Count() == 3)
                            .Sum(g => g.Key * 3);

            else damage = numberCards.Sum(c => c.value);

            damage += (int)rankings.First();

            // == 데미지 곱 연산 ==
            if (context.PreviousResult.isSet)
            {
                if (context.PreviousResult.IsRedCardOver3)
                    damage *= context.Multiply.red3multiply;

                if (context.PreviousResult.IsBlackCardOver3)
                    damage *= context.Multiply.black3multiply;
            }

            // == 데미지 강화 카드의 존재 여부 ==

            // == 유물 데미지 판정 ==

            var result = new CardResult(damage, directions, rankings, isRedCardOver3, isBlackCardOver3);
            context.SetResult(result);
        }



        #region 카드 족보 판정

        private static List<HandRanking> CalculateRanking(List<NumberCardData> cards)
        {
            var rankings = new List<HandRanking>();

            if (IsStraightFlush(cards))
                rankings.Add(HandRanking.StraightFlush);
            if (IsFourCard(cards))
                rankings.Add(HandRanking.FourCard);
            if (IsStraight(cards))
                rankings.Add(HandRanking.Straight);
            if (IsFlush(cards))
                rankings.Add(HandRanking.Flush);
            if (IsTriple(cards))
                rankings.Add(HandRanking.Triple);
            if (IsTwoPair(cards))
                rankings.Add(HandRanking.TwoPair);
            if (IsOnePair(cards))
                rankings.Add(HandRanking.OnePair);
            if (rankings.Count() == 0)
                rankings.Add(HandRanking.High);

            return rankings;
        }

        private static bool IsStraight(List<NumberCardData> cards)
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

        private static bool IsFlush(List<NumberCardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.Select(c => c.color)
                    .Distinct()
                    .Count() == 1;
        }

        private static bool IsStraightFlush(List<NumberCardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return IsStraight(cards) && IsFlush(cards);
        }

        static bool IsFourCard(List<NumberCardData> cards)
        {
            return cards.GroupBy(c => c.value)
                        .Any(g => g.Count() == 4);
        }

        static bool IsTriple(List<NumberCardData> cards)
        {
            return cards.GroupBy(c => c.value)
                        .Any(g => g.Count() == 3);
        }

        static bool IsTwoPair(List<NumberCardData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.GroupBy(c => c.value)
                    .Where(g => g.Count() == 2)
                    .Count() == 2;
        }

        static bool IsOnePair(List<NumberCardData> cards)
        {
            return cards.GroupBy(c => c.value)
                        .Any(g => g.Count() == 2);
        }

        #endregion
    }
}