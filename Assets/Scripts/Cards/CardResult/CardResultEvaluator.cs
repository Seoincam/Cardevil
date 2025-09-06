using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Cards
{
    public static class CardResultEvaluator
    {
        /// <summary>
        /// 카드를 사용했을 때, 계산 후 context에 저장
        /// </summary>
        public static void SetResult(CardContext context, IEnumerable<Card> cards)
        {
            var result = Evaluate(context, cards);
            context.SetResult(result);
        }

        /// <summary>
        /// 선택된 카드를 바탕으로 계산 후 반환
        /// </summary>
        public static CardResult Evaluate(CardContext context, IEnumerable<Card> cards)
        {
            var moves = cards.Where(c => c.data.valueType == CardData.ValueType.Move)
                        .Select(m => m.data.Move)
                        .ToList();

            var numbers = cards.Where(c => c.data.valueType == CardData.ValueType.Number)
                        .Select(n => n.data.Number)
                        .ToList();


            // == 숫자 카드 판정 ==
            if (numbers.Count == 0)
            {
                var ranking = new List<HandRanking>() { HandRanking.None };
                return new CardResult(0, moves, ranking, numbers);
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

            var rankings = CalculateRanking(numbers);
            float damage = 0;

            if (rankings[0] == HandRanking.High)
                damage = numbers.OrderBy(n => n.number).Last().number;

            else if (rankings[0] == HandRanking.OnePair)
                damage = numbers.GroupBy(n => n.number)
                            .Where(g => g.Count() == 2)
                            .Sum(g => g.Key * 2);

            else if (rankings[0] == HandRanking.Triple)
                damage = numbers.GroupBy(n => n.number)
                            .Where(g => g.Count() == 3)
                            .Sum(g => g.Key * 3);

            else damage = numbers.Sum(n => n.number);

            damage += (int)rankings[0];

            // == 데미지 곱 연산 ==
                //TODO: 수치들 하드코딩 제거
            if (context.PreviousResult.IsRedFlush)
                damage *= 3;

            // == 데미지 강화 카드의 존재 여부 ==

            // == 유물 데미지 판정 ==

            var result = new CardResult(damage, moves, rankings, numbers);

            return result;
        }



        #region 카드 족보 판정

        private static List<HandRanking> CalculateRanking(List<NumberData> numbers)
        {
            var rankings = new List<HandRanking>();

            if (IsStraightFlush(numbers))
                rankings.Add(HandRanking.StraightFlush);
            if (IsFourCard(numbers))
                rankings.Add(HandRanking.FourCard);
            if (IsStraight(numbers))
                rankings.Add(HandRanking.Straight);
            if (IsFlush(numbers))
                rankings.Add(HandRanking.Flush);
            if (IsTriple(numbers))
                rankings.Add(HandRanking.Triple);
            if (IsTwoPair(numbers))
                rankings.Add(HandRanking.TwoPair);
            if (IsOnePair(numbers))
                rankings.Add(HandRanking.OnePair);
            if (rankings.Count() == 0)
                rankings.Add(HandRanking.High);

            return rankings;
        }

        private static bool IsStraight(List<NumberData> cards)
        {
            if (cards.Count != 4)
                return false;

            var values = cards.Select(c => c.number)
                    .OrderBy(v => v)
                    .ToList();

            for (int i = 1; i < cards.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;

            return true;
        }

        private static bool IsFlush(List<NumberData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.Select(c => c.color)
                    .Distinct()
                    .Count() == 1;
        }

        private static bool IsStraightFlush(List<NumberData> cards)
        {
            if (cards.Count != 4)
                return false;

            return IsStraight(cards) && IsFlush(cards);
        }

        static bool IsFourCard(List<NumberData> cards)
        {
            return cards.GroupBy(c => c.number)
                        .Any(g => g.Count() == 4);
        }

        static bool IsTriple(List<NumberData> cards)
        {
            return cards.GroupBy(c => c.number)
                        .Any(g => g.Count() == 3);
        }

        static bool IsTwoPair(List<NumberData> cards)
        {
            if (cards.Count != 4)
                return false;

            return cards.GroupBy(c => c.number)
                    .Where(g => g.Count() == 2)
                    .Count() == 2;
        }

        static bool IsOnePair(List<NumberData> cards)
        {
            return cards.GroupBy(c => c.number)
                        .Any(g => g.Count() == 2);
        }

        #endregion
    }
}