using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Relics;
using Database.Generated;
using UnityEngine;

namespace Cardevil.Cards
{
    public static class CardResultEvaluator
    {
        /// <summary>
        /// 카드 사용시 사용하는 EvaluationSet List를 계산 후 반환.
        /// </summary>
        public static List<EvaluationSet> GetEvaluationSets(CardResultContext resultCtx, List<Card> cards, out CardResult result)
        {
            resultCtx.StepToNext();
            
            var sets = new List<EvaluationSet>();

            // move only 체크
            var numbers = cards.Where(c => c.data.valueType == CardData.ValueType.Number).ToList();
            var moves = cards.Where(c => c.data.valueType == CardData.ValueType.Move).ToList();
            if (numbers.Count == 0)
            {
                sets.Add(new EvaluationSet(null, EffectDamageType.None, 0));
                result = new(resultCtx, new() { HandRanking.None }, numbers, moves);
                return sets;
            }


            // 족보
            var ranking = GetRanking(cards);
            var rankingData = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(d => d.Ranking == ranking);
            result = new(resultCtx, CalculateRanking(numbers), numbers, moves);

            // 기본 족보 보너스
            sets.Add(new EvaluationSet(null, EffectDamageType.Plus, rankingData?.Value ?? 0));

            // 기본 데미지
            switch (ranking)
            {
                case HandRanking.High:
                    var top = numbers.OrderBy(n => n.data.Number.NumberValue).Last();
                    var damage = top.data.Number.NumberValue;
                    sets.Add(new EvaluationSet(top, EffectDamageType.Plus, damage));
                    break;

                case HandRanking.OnePair:
                    var pairCards = numbers.GroupBy(n => n.data.Number.NumberValue)
                            .Where(g => g.Count() == 2)
                            .SelectMany(g => g)
                            .ToList();
                    foreach (var card in pairCards)
                        sets.Add(new EvaluationSet(card, EffectDamageType.Plus, card.data.Number.NumberValue));
                    break;

                case HandRanking.Triple:
                    var tripleCards = numbers.GroupBy(n => n.data.Number.NumberValue)
                            .Where(g => g.Count() == 3)
                            .SelectMany(g => g)
                            .ToList();
                    foreach (var card in tripleCards)
                        sets.Add(new EvaluationSet(card, EffectDamageType.Plus, card.data.Number.NumberValue));
                    break;

                default:
                    foreach (var card in numbers)
                        sets.Add(new EvaluationSet(card, EffectDamageType.Plus, card.data.Number.NumberValue));
                    break;
            }

            // TODO: 추가 데미지

            // 유물
            var relics = RelicDataManager.Instance;
            if (relics == null)
            {
                Debug.LogWarning("[EvaluateResult] RelicDataManager.Instance is null");
                return sets;
            }

            var effects = relics.GetPlayerEffect(EffectType.OnEvaluation)
                .Select(e => e.OnEvaluationData)
                .Where(e => e != null)
                .ToList();

            int Priority(EffectDamageType type) => type switch
            {
                EffectDamageType.MultiplyRanking => 0,
                EffectDamageType.Plus => 1,
                EffectDamageType.MultiplyAll => 2,
                _ => 99
            };

            foreach (var e in effects.OrderBy(e => Priority(e.EffectType)))
            {
                if (CanTrigger(e, ranking, resultCtx))
                    sets.Add(new EvaluationSet(null, e.EffectType, e.EffectValue));
            }

            return sets;
        }


        /// <summary>
        /// data의 조건들을 바탕으로 실행 가능한지 체크.
        /// </summary>
        public static bool CanTrigger(RelicEffectOnEvaluationData data, HandRanking ranking, CardResultContext resultCtx)
        {
            // 기본 방어
            if (data == null) return false;

            // 확률
            if (data.Possibility < Random.value)
                return false;

            // HP (0이면 무시)
            if (data.TriggerHp != 0 &&
                Managers.Game?.PlayerStatus?.CurrentHp != data.TriggerHp)
                return false;

            switch (data.ExecuteType)
            {
                case EffectExcuteType.immediate:
                    // TriggerRanking == None이면 모든 랭킹 허용
                    if (data.TriggerRanking != HandRanking.None &&
                        data.TriggerRanking != ranking)
                        return false;
                    return true;

                case EffectExcuteType.Next:
                    // 현재 랭킹이 타겟에 포함되어야 함
                    if (data.TargetRankings.Count == 0 || !data.TargetRankings.Contains(ranking))
                        return false;

                    int remaining = data.ExecutionCount;

                    // 최근부터 과거로 스캔하는 방식으로 체크
                    for (int i = resultCtx.History.Count - 1; i >= 0; i--)
                    {
                        var result = resultCtx.History[i];
                        if (result is not { } r) continue;

                        if (r.Ranking == data.TriggerRanking)
                            return remaining > 0;

                        if (data.TargetRankings.Contains(r.Ranking))
                        {
                            remaining--;
                            if (remaining <= 0) return false;
                        }
                    }
                    return false;

                case EffectExcuteType.Permanent:
                    // 현재 랭킹이 타겟에 포함
                    if (data.TargetRankings.Count == 0 || !data.TargetRankings.Contains(ranking))
                        return false;

                    return resultCtx.History.Any(r => r?.Ranking == data.TriggerRanking);

                case EffectExcuteType.None:
                default:
                    return false;
            }
        }




        public struct EvaluationSet
        {
            public readonly IEvaluateAction Action;
            public readonly EffectDamageType Type;
            public readonly float Value;

            public EvaluationSet(IEvaluateAction actions, EffectDamageType type, float value)
            {
                Action = actions;
                Type = type;
                Value = value;
            }
        }

        /// <summary>
        /// 카드를 사용했을 때 반응해야 할 개체가 구현. 
        /// </summary>
        public interface IEvaluateAction
        {
            /// <summary>
            /// Evaluation 시 반응.
            /// </summary>
            public void ExecuteEvaluationAction();
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