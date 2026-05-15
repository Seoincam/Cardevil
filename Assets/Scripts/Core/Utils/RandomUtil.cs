using Cardevil.Test.DebugConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using UMRandom = Unity.Mathematics.Random;
using URandom = UnityEngine.Random;

namespace Cardevil.Core.Utils
{
    public static class RandomUtil
    {
        public enum RandomType
        {
            Default = 0,
            CardShuffle = 1,
            Events = 2,
            
            MAX
        }
        
        private static Dictionary<RandomType, UMRandom> randoms = new Dictionary<RandomType, UMRandom>();
        private static Dictionary<RandomType, uint> randomInitialSeeds = new Dictionary<RandomType, uint>();
        
        
        private static bool _isInitialized = false;
        
        public static void Init()
        {
            _isInitialized = true;
            for (RandomType randomType = RandomType.Default; randomType < RandomType.MAX; randomType++)
            {
                InitSeed(randomType);
            }
        }

        public static void InitSeed(RandomType type, uint initialSeed = 0, uint setSeed = 0)
        {
            if (setSeed == 0)
            {
                if (initialSeed == 0)
                {
                    initialSeed = (uint)URandom.Range(1, int.MaxValue);
                }
                setSeed = initialSeed;
            }
            randoms[type] = new UMRandom(setSeed);
            randomInitialSeeds[type] = initialSeed;
        }
        
        public static int GetRandomInt(int min, int max, RandomType type = RandomType.Default)
        {
            if (!randoms.TryGetValue(type, out var r))
            {
                InitSeed(type);
            }

            var random = randoms[type];
            int result = random.NextInt(min, max);
            randoms[type] = random; // UMRandom이 struct이기 때문.
            
            return result;
        }

        /// <summary>
        /// Returns a random float within [0.0..1.0]
        /// </summary>
        public static float GetValue(RandomType type = RandomType.Default)
        {
            return GetRandomFloat(0, 1, type);
        }
        
        public static float GetRandomFloat(float min, float max, RandomType type = RandomType.Default)
        {
            if (!randoms.TryGetValue(type, out var r))
            {
                InitSeed(type);
            }
            
            var random = randoms[type];
            float result = random.NextFloat(min, max);
            randoms[type] = random; // UMRandom이 struct이기 때문.
            
            return result;
        }
        
        /// <summary>
        /// <see cref="RandomType"/> 기반의 Fisher–Yates in place 셔플.  
        /// </summary>
        public static void ShuffleListInPlace<T>(this IList<T> list, RandomType type = RandomType.CardShuffle)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = GetRandomInt(0, i + 1, type);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
        
        
        public static int WeightedRandomIndex(IEnumerator<int> weights, RandomType type = RandomType.Default)
        {
            if (weights == null) throw new ArgumentNullException(nameof(weights));

            int totalWeight = 0;
            List<int> weightList = new List<int>();
            while (weights.MoveNext())
            {
                int weight = weights.Current;
                if (weight < 0) throw new ArgumentException("Weights cannot be negative.", nameof(weights));
                totalWeight += weight;
                weightList.Add(weight);
            }

            if (totalWeight == 0) throw new ArgumentException("Total weight must be greater than zero.", nameof(weights));

            int randomValue = GetRandomInt(0, totalWeight, type);
            for (int i = 0; i < weightList.Count; i++)
            {
                if (randomValue < weightList[i])
                {
                    return i;
                }
                randomValue -= weightList[i];
            }

            // Should never reach here if totalWeight > 0
            throw new InvalidOperationException("Failed to select a weighted random index.");
        }
        
        public static int WeightedRandomIndex(IEnumerable<int> weights, RandomType type = RandomType.Default)
        {
            return WeightedRandomIndex(weights.GetEnumerator(), type);
        }
        
        public static int WeightedRandomIndex(params int[] weights)
        {
            return WeightedRandomIndex(weights.AsEnumerable());
        }

        [Serializable]
        public class RandomSave
        {
            public RandomType type;
            public uint initialSeed;
            public uint state;
        }
        
        public static List<RandomSave> SerializeRandoms()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("RandomUtil is not initialized. Call Init() first.");
            }
            
            List<RandomSave> saves = new List<RandomSave>();
            foreach (var kvp in randoms)
            {
                saves.Add(new RandomSave
                {
                    type = kvp.Key,
                    initialSeed = randomInitialSeeds[kvp.Key],
                    state = kvp.Value.state
                });
            }
            return saves;
        }
        
        public static void DeserializeRandoms(List<RandomSave> saves)
        {
            foreach (var save in saves)
            {
                InitSeed(save.type, save.initialSeed, save.state);
            }
            _isInitialized = true;
        }
    }
    
    public class RandomUtilShuffleTest
    {
        /*
         * 카드 셔플을 하면 많은 경우
         * Red 2, 3, 4, 5가 포함되어 있다고 느껴졌음.
         * 검증을 위해 ShuffleTest 클래스를 만듦.
         */
        
        [ConsoleCommand("testShuffle")]
        public static void Test()
        {
            int deckSize = 50, handSize = 6, trialCount = 500_000;
            int[] buckets = new int[5]; // id 0~3 중, 0~4장 포함 카운트
            
            for (int t = 0; t < trialCount; t++)
            {
                var deck = Enumerable.Range(0, deckSize).ToList();
                deck.ShuffleListInPlace(); // 테스트 위해 셔플마다 시드 삭제 후 InitSeed()

                int cnt = deck.Take(handSize).Count(id => id < 4);
                buckets[cnt]++;
            }

            for (int k = 0; k <= 4; k++)
                LogEx.Log($"{k}장 포함: {(double)buckets[k]/trialCount*100:F3}%");

            double rate = (double)buckets[4]/trialCount*100;
            LogEx.Log($"모두 포함 확률: {rate:F5}%");
        }
    }
}