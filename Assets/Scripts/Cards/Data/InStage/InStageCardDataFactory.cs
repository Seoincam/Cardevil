using Cardevil.Cards.Data.Modifiers;
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
        public static List<BuiltCardData> BuildInStageCardData(
            IReadOnlyDictionary<int, CardPipeline> source,
            bool shuffle = true,
            int? seed = null)
        {
            List<BuiltCardData> result = new(source.Count);

            foreach (var pipeline in source)
            {
                if (pipeline.Value == null)
                {
                    LogEx.LogError("null CardData가 전달됨.");
                    continue;
                }

                var builder = BuiltCardData.StartBuild(pipeline.Key, pipeline.Value.Kind);
                
                // TODO: 순서 및 로직
                foreach(var mod in pipeline.Value.Modifiers)
                    mod.Apply(builder);
                
                // TODO: 검증
                
                result.Add(builder.Build());
            }

            if (shuffle)
            {
                if (seed.HasValue)
                {
                    var rng = new Random(seed.Value);
                    result.ShuffleInPlace(rng); // 재현 가능한 셔플
                }
                else
                {
                    result.ShuffleInPlaceUnity(); // Unity 랜덤 셔플
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