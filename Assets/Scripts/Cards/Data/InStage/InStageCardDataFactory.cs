using Cardevil.Utils;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public static class InStageCardDataFactory
    {
        /// <summary>
        /// CardData 목록을 InStageCardData로 변환하고 필요 시 셔플.
        /// </summary>
        /// <param name="source">원본 카드 데이터들</param>
        /// <param name="shuffle">셔플 여부</param>
        /// <param name="seed">재현 가능한 셔플을 위한 시드 (null이면 랜덤)</param>
        public static List<InStageCardData> BuildInStageCardData(
            IReadOnlyCollection<CardPipeline> source,
            bool shuffle = true,
            int? seed = null)
        {
            var result = new List<InStageCardData>(source.Count);

            foreach (var origin in source)
            {
                if (origin == null)
                {
                    LogEx.LogError("null CardData가 전달됨.");
                    continue;
                }

                // Number/Move 둘 다 있거나 둘 다 없는 경우 방어
                bool hasNumber = origin.NumberPipeline != null;
                bool hasMove   = origin.MovePipeline   != null;

                if (hasNumber == hasMove) // 둘 다 true 또는 둘 다 false
                {
                    LogEx.LogError($"잘못된 Modifier 구성 (Id:{origin.Id}) — Number/Move가 둘 다 있거나 둘 다 없음.");
                    continue;
                }

                if (hasNumber)
                {
                    var builtNumber = origin.NumberPipeline.Build();
                    result.Add(InStageCardData.FromNumber(origin.Id, builtNumber));
                }
                else // hasMove
                {
                    var builtMove = origin.MovePipeline.Build();
                    result.Add(InStageCardData.FromMove(origin.Id, builtMove));
                }
            }

            if (shuffle)
            {
                if (seed.HasValue)
                {
                    var rng = new Random(seed.Value);
                    result.ShuffleInPlace(rng);          // 재현 가능한 셔플
                }
                else
                {
                    result.ShuffleInPlaceUnity();        // Unity 랜덤 셔플
                }
            }

            return result;
        }
    }
    
    public static class DeckShuffleExtensions
    {
        /// <summary>
        /// System.Random 기반의 제자리 셔플 (재현 가능).
        /// </summary>
        public static void ShuffleInPlace<T>(this IList<T> list, Random rng)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>
        /// UnityEngine.Random 기반의 제자리 셔플 (재현 불가).
        /// </summary>
        public static void ShuffleInPlaceUnity<T>(this IList<T> list)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}