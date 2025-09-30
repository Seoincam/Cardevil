using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.Interactions;
using Cardevil.Relics;
using UnityEngine;

namespace Cardevil.Cards.Evaluations
{
    public static class CardResultEvaluator
    {
        /// <summary>
        /// 미리 카드 결과를 계산 후, AsncEvaluationEvent에 등록.
        /// </summary>
        public static void PreEvaluate(List<Card> cards)
        {
            CardResult result;

            // move only
            cards.Sort((a, b) => a.HandIndex.CompareTo(b.HandIndex));
            var numbers = cards.Where(c => c.data.valueType == CardData.ValueType.Number).ToList();
            var moves = cards.Where(c => c.data.valueType == CardData.ValueType.Move).ToList();

            if (numbers.Count == 0)
            {
                using (var move = EvaluationAction.Get())
                {
                    move.SetValue(priority: -100, EffectEvaluation.None);
                    move.SetVisual(moves);
                }

                result = new(numbers, moves);
                Managers.Card.ResultCtx.Commmit(result);
                return;
            }

            // 족보
            var rankings = CalculateRanking(numbers);
            var primaryRanking = rankings[0];
            result = new(rankings, numbers, moves);
            Managers.Card.ResultCtx.Commmit(result);

            // 기본 족보 보너스 
            // priority 100번대
            if (primaryRanking > HandRanking.High)
            {
                using (var r = EvaluationAction.Get())
                {
                    var rankingData = Managers.Database.Database.HandRankingDataList
                        .FirstOrDefault(d => d.Ranking == primaryRanking);
                    r.SetValue(priority: 100, EffectEvaluation.Plus, rankingData?.Value ?? 0);
                    r.SetVisual(numbers);
                }
            }


            // 기본 데미지
            // 4장의 카드를 쓰지 않는 경우를 계산
            // priority 200번대
            var p = 200;
            switch (primaryRanking)
            {
                case HandRanking.High:
                    var top = numbers.Aggregate((best, cur) =>
                        cur.data.Number.NumberValue > best.data.Number.NumberValue ? cur : best);
                    using (var high = EvaluationAction.Get())
                    {
                        high.SetValue(priority: p++, EffectEvaluation.Plus, top.data.Number.NumberValue);
                        high.SetVisual(top);
                    }
                    break;

                case HandRanking.OnePair:
                    AddEventByCount(numbers, count: 2);
                    break;

                case HandRanking.Triple:
                    AddEventByCount(numbers, count: 3);
                    break;

                default:
                    foreach (var card in numbers)
                    {
                        using (var c = EvaluationAction.Get())
                        {
                            c.SetValue(priority: p++, EffectEvaluation.Plus, card.data.Number.NumberValue);
                            c.SetVisual(card);
                        }
                    }
                    break;
            }

            // 원페어, 트리플 등록 메서드.
            void AddEventByCount(List<Card> cards, int count)
            {
                foreach (var card in cards
                    .GroupBy(n => n.data.Number.NumberValue)
                    .Where(g => g.Count() == count)
                    .SelectMany(g => g))
                {
                    var val = card.data.Number.NumberValue;
                    using (var act = EvaluationAction.Get())
                    {
                        act.SetValue(priority: p++, EffectEvaluation.Plus, val);
                        act.SetVisual(card);
                    }
                }
            }



            // TODO: 추가 데미지

            // 유물
            var relics = RelicDataManager.Instance;
            if (relics == null)
            {
                Debug.LogWarning("[EvaluateResult] RelicDataManager.Instance is null");
                return;
            }

            var effects = relics.GetPlayerEffect(EffectType.OnEvaluation)
                .Where(e => e.EffectType == EffectType.OnEvaluation)
                .ToList();

            int Priority(EffectEvaluation type) => type switch
            {
                EffectEvaluation.MultiplyRanking => 0,
                EffectEvaluation.Plus => 1,
                EffectEvaluation.MultiplyAll => 2,
                _ => 99
            };

            // TODO: 순서 더 정확히
            var pr = 300;
            foreach (var e in effects.OrderBy(e => Priority(e.OnEvaluationData.EvaluationType)))
            {
                if (e.CanTriggerOnEvaluation(primaryRanking))
                {
                    using (var r = EvaluationAction.Get())
                    {
                        r.SetValue(priority: pr++, e.OnEvaluationData.EvaluationType, e.OnEvaluationData.EffectValue);
                        // r.SetVisual();
                    }
                }
            }

            return;
        }





        #region 카드 족보 판정

        /// <summary>
        /// 카드를 바탕으로 '가장 상위' 족보를 계산해서 반환.
        /// 숫자 카드가 없을 시 None 반환.
        /// </summary>
        public static HandRanking GetRanking(List<Card> cards)
        {
            var numbers = cards.Where(c => c.data.valueType == CardData.ValueType.Number)
                    .ToList();

            if (numbers.Count == 0)
                return HandRanking.None;

            var rankings = CalculateRanking(numbers);
            return rankings[0];
        }

        /// <summary>
        /// 숫자 카드를 바탕으로 '모든' 족보를 반환. 
        /// </summary>
        private static List<HandRanking> CalculateRanking(List<Card> numberCards)
        {
            var rankings = new List<HandRanking>();

            var numberDatas = numberCards.Where(c => c.data.valueType == CardData.ValueType.Number)
                .Select(c => c.data)
                .ToList();

            if (IsStraightFlush(numberDatas))
                rankings.Add(HandRanking.StraightFlush);
            if (IsFourCard(numberDatas))
                rankings.Add(HandRanking.FourCard);
            if (IsStraight(numberDatas))
                rankings.Add(HandRanking.Straight);
            if (IsFlush(numberDatas))
            {
                var ranking = numberDatas[0].Number.ColorValue switch
                {
                    NumberData.CardColor.Red => HandRanking.RedFlush,
                    NumberData.CardColor.Green => HandRanking.GreenFlush,
                    NumberData.CardColor.Blue => HandRanking.BlueFlush,
                    NumberData.CardColor.Black => HandRanking.BlackFlush,
                    _ => HandRanking.None
                };
                if (ranking == HandRanking.None)
                    Debug.LogError("Flush 분류에 실패했습니다.");

                rankings.Add(ranking);
            }
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