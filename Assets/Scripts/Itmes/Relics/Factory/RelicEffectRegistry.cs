using Cardevil.Relics;
using Cardevil.Relics.OnEvaluation;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Cardevil.Items.Relics.Factory
{
    public static class RelicEffectRegistry
    {
        private static readonly Dictionary<string, IRelicEffectFactory> Map = new();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // OnEvaluation Type
            Register("ByHandRanking", new ByHandRankingFactory());
            Register("NextByHandRanking", new NextByHandRankingFactory());
            Register("Randomly", new RandomlyFactory());
            Register("ByHp", new ByHpFactory());
            Register("OnEachCard", new OnEachCardFactory());
        }
        
        private static void Register(string effectType, IRelicEffectFactory factory) => Map[effectType] = factory;

        /// <summary>
        /// OnEvaluation 타입의 Relic Effect를 생성.
        /// </summary>
        public static RelicEffectBase Create(RelicEffectSpec spec)
        {
            if (!Map.TryGetValue(spec.EffectType, out var f))
                throw new InvalidDataException($"the effect factory of {spec.EffectType} type is not registered");

            return f.Create(spec);
        }
        
        private interface IRelicEffectFactory
        {
            RelicEffectBase Create(RelicEffectSpec spec);
        }

        private sealed class ByHandRankingFactory : IRelicEffectFactory
        {
            public RelicEffectBase Create(RelicEffectSpec spec)
            {
                return new DamageByHandRankingEffect(spec.EffectId, spec.TriggerHandRanking, spec.IsPlus,
                    spec.EffectValue);
            }
        }

        private sealed class ByHpFactory : IRelicEffectFactory
        {
            public RelicEffectBase Create(RelicEffectSpec spec)
            {
                return new DamageByHpEffect(spec.EffectId, spec.TriggerHp, spec.IsPlus, spec.EffectValue);
            }
        }

        private sealed class NextByHandRankingFactory : IRelicEffectFactory
        {
            public RelicEffectBase Create(RelicEffectSpec spec)
            {
                return new DamageNextByHandRankingEffect(spec.EffectId, spec.TriggerHandRanking, spec.IsPermanent,
                    spec.ExecutionCount, spec.IsPlus, spec.EffectValue);
            }
        }

        private sealed class OnEachCardFactory : IRelicEffectFactory
        {
            public RelicEffectBase Create(RelicEffectSpec spec)
            {
                return new DamageOnEachCardEffect(spec.EffectId, spec.IsBasedKillCount, (int)spec.EffectValue);
            }
        }

        private sealed class RandomlyFactory : IRelicEffectFactory
        {
            public RelicEffectBase Create(RelicEffectSpec spec)
            {
                return new DamageRandomlyEffect(spec.EffectId, spec.TriggerPossibility, spec.IsPlus, spec.EffectValue);
            }
        }
    }
}