
using System;
using System.Collections.Generic;
using UMRandom = Unity.Mathematics.Random;
using URandom = UnityEngine.Random;

namespace Cardevil.Utils
{
    public static class RandomUtil
    {
        public enum RandomType
        {
            Default = 0,
            CardShuffle, 
            
            MAX
        }
        
        private static Dictionary<RandomType, UMRandom> randoms = new Dictionary<RandomType, UMRandom>();
        private static Dictionary<RandomType, uint> randomSeeds = new Dictionary<RandomType, uint>();
        
        
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
                setSeed = initialSeed == 0 ? (uint) URandom.Range(int.MinValue, int.MaxValue) : initialSeed;
            }
            randoms[type] = new UMRandom(setSeed);
            randomSeeds[type] = setSeed;
        }
        
        public static int GetRandomInt(int min, int max, RandomType type = RandomType.Default)
        {
            if (!randoms.TryGetValue(type, out var r))
            {
                InitSeed(type);
            }
            return randoms[type].NextInt(min, max);
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
                    initialSeed = randomSeeds[kvp.Key],
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
}