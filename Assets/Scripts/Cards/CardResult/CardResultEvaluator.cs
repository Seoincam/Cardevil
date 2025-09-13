using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Cards
{
    public static class CardResultEvaluator
    {   
        /// <summary>
        /// 유물, 아이템 등의 효과 종류.
        /// </summary>
        public enum EffectType
        {
            Damage, // 데미지 계산 시점에 적용
            Ranking, // 유물 계산 시점에 적용
            Final // 최종 계산 시점에 적용
        }


        /// <summary>
        /// 카드를 사용했을 때, 계산 후 context에 저장
        /// </summary>
        public static void SetResult(CardResultContext ctx, IEnumerable<Card> cards)
        {
            var result = Evaluate(ctx, cards);
            ctx.SetResult(result);
        }


        /// <summary>
        /// 선택된 카드를 바탕으로 계산 후 결과를 반환
        /// </summary>
        public static CardResult Evaluate(CardResultContext ctx, IEnumerable<Card> cards)
        {
            var datas = cards.Select(c => c.data)
                    .ToList();

            var numbers = datas.Where(c => c.valueType == CardData.ValueType.Number)
                    .ToList();

            var moves = datas.Where(c => c.valueType == CardData.ValueType.Move)
                    .ToList();



            // 숫자 카드 판정 
            if (numbers.Count == 0)
            {
                var ranking = new List<HandRanking>() { HandRanking.None };
                return new CardResult(ctx, damage: 0, ranking, datas, numbers, moves);
            }

            // 데미지 계산
            foreach (var numberData in numbers)
            {
                var baseDamage = numberData.Number.NumberValue + numberData.AdditionalDamage;
                numberData.Number.Damage = baseDamage;
            }
                

            // -> 유물 효과 적용



            var rankings = CalculateRanking(numbers);
            float finalDamage = rankings[0] switch
            {
                HandRanking.High => numbers.OrderBy(n => n.Number.NumberValue).Last().Number.Damage,

                HandRanking.OnePair => numbers.GroupBy(n => n.Number.NumberValue)
                            .Where(g => g.Count() == 2)
                            .Sum(g => g.Sum(n => n.Number.Damage)),                           

                HandRanking.Triple => numbers.GroupBy(n => n.Number.NumberValue)
                            .Where(g => g.Count() == 3)
                            .Sum(g => g.Sum(n => n.Number.Damage)),

                _ => numbers.Sum(n => n.Number.Damage)
            };

            finalDamage += (int)rankings[0];

            // == 데미지 곱 연산 ==
            //TODO: 수치들 하드코딩 제거
            if (ctx.IsBlackFlushUsed)
                finalDamage *= 2;

            if (ctx.PreviousResult.IsRedFlush)
                finalDamage *= 3;

            // == 데미지 강화 카드의 존재 여부 ==

            // == 유물 데미지 판정 ==

            var result = new CardResult(ctx, finalDamage, rankings, datas, numbers, moves);

            return result;
        }



        #region 카드 족보 판정

        private static List<HandRanking> CalculateRanking(List<CardData> numberDatas)
        {
            var rankings = new List<HandRanking>();

            if (IsStraightFlush(numberDatas))
                rankings.Add(HandRanking.StraightFlush);
            if (IsFourCard(numberDatas))
                rankings.Add(HandRanking.FourCard);
            if (IsStraight(numberDatas))
                rankings.Add(HandRanking.Straight);
            if (IsFlush(numberDatas))
                rankings.Add(HandRanking.Flush);
            if (IsTriple(numberDatas))
                rankings.Add(HandRanking.Triple);
            if (IsTwoPair(numberDatas))
                rankings.Add(HandRanking.TwoPair);
            if (IsOnePair(numberDatas))
                rankings.Add(HandRanking.OnePair);
            if (rankings.Count() == 0)
                rankings.Add(HandRanking.High);

            return rankings;
        }

        private static bool IsStraight(List<CardData> numberDatas)
        {
            if (numberDatas.Count != 4)
                return false;

            var values = numberDatas.Select(c => c.Number.NumberValue)
                    .OrderBy(v => v)
                    .ToList();

            for (int i = 1; i < numberDatas.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;

            return true;
        }

        private static bool IsFlush(List<CardData> numberDatas)
        {
            if (numberDatas.Count != 4)
                return false;

            return numberDatas.Select(c => c.Number.ColorValue)
                    .Distinct()
                    .Count() == 1;
        }

        private static bool IsStraightFlush(List<CardData> numberDatas)
        {
            if (numberDatas.Count != 4)
                return false;

            return IsStraight(numberDatas) && IsFlush(numberDatas);
        }

        static bool IsFourCard(List<CardData> numberDatas)
        {
            return numberDatas.GroupBy(c => c.Number.NumberValue)
                        .Any(g => g.Count() == 4);
        }

        static bool IsTriple(List<CardData> numberDatas)
        {
            return numberDatas.GroupBy(c => c.Number.NumberValue)
                        .Any(g => g.Count() == 3);
        }

        static bool IsTwoPair(List<CardData> numberDatas)
        {
            if (numberDatas.Count != 4)
                return false;

            return numberDatas.GroupBy(c => c.Number.NumberValue)
                    .Where(g => g.Count() == 2)
                    .Count() == 2;
        }

        static bool IsOnePair(List<CardData> numberDatas)
        {
            return numberDatas.GroupBy(c => c.Number.NumberValue)
                        .Any(g => g.Count() == 2);
        }

        #endregion
    }
}