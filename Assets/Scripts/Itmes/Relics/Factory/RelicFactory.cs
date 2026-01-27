using Cardevil.Core.Bootstrap;
using Cardevil.Relics;
using Cardevil.Relics.OnEvaluation;
using Cardevil.Utils;
using Database;
using Database.Generated;
using System;
using System.Collections.Generic;
using System.IO;

namespace Cardevil.Items.Relics.Factory
{
    public static class RelicFactory
    {
        private static readonly Dictionary<string, IRelicEffectFactory> EffectFactoryMap = new();
        
        public static void RegisterAllFactory()
        {
            // OnEvaluation Type
            Register("ByHandRanking", new ByHandRankingFactory());
            Register("NextByHandRanking", new NextByHandRankingFactory());
            Register("Randomly", new RandomlyFactory());
            Register("ByHp", new ByHpFactory());
            Register("OnEachCard", new OnEachCardFactory());
        }
        
        public static List<Relic> MakeRelicInstances()
        {
            // DB 검증
            var db = CardevilCore.Instance.Database.Database;
            var list = db.RelicDataList;
            if (list == null || list.Count == 0)
            {
                LogEx.LogError("There is no RelicData list!");
                return new List<Relic>();
            }
            
            Dictionary<string, RelicEffectBase> effectMap = MakeEffectMap();
            
            int created = 0, skipped = 0;
            var relics = new List<Relic>(list.Count);
            foreach (var d in list)
            {
                // Relic Data 검증
                if (d == null)
                {
                    LogEx.LogWarning($"Encountered null relic data. Skip {d.RelicId}.");
                    skipped++;
                    continue;
                }

                // Relic Id 검증
                if (string.IsNullOrWhiteSpace(d.RelicId))
                {
                    LogEx.LogWarning($"RelicId is null or empty. Skip {d.RelicId}.");
                    skipped++;
                    continue;
                }

                if (d.EffectIds == null || d.EffectIds.Count == 0)
                {
                    LogEx.LogWarning($"Relic effect id is null or empty. Skip {d.RelicId}.");
                    skipped++;
                    continue;
                }

                // Effect Id로 조회 및 추가
                var effects = new List<RelicEffectBase>(d.EffectIds.Count);
                var seen = new HashSet<string>(); // 중복 방지
                bool hasError = false;
                foreach (var effectId in d.EffectIds)
                {
                    // Effect Id 검증
                    var id = effectId.Trim('"', ' ');
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        LogEx.LogWarning($"Relic effect id is null or empty. Skip {id}");
                        hasError = true;
                        break;
                    }

                    if (!seen.Add(id))
                    {
                        LogEx.LogWarning("Relic effect id is already in use.");
                        // 중복은 일단 그냥 넘어감.
                    }

                    if (!effectMap.TryGetValue(id, out var effect))
                    {
                        LogEx.LogError($"Effect({id}) is not found or null in effectMap. Skip {id}.");
                        hasError = true;
                        break;
                    }
                    
                    effects.Add(effect);
                }

                if (hasError)
                {
                    skipped++;
                    continue;
                }

                // Relic 생성 시도
                string relicId = d.RelicId.Trim('"', ' ');
                try
                {
                    Relic relic = new(relicId, d.Level, d.Rarity, d.DisplayName, d.DisplayDescription, effects);

                    relics.Add(relic);
                    created++;
                }
                catch (Exception ex)
                {
                    LogEx.LogError($"Failed to create Relic '{relicId}' : {ex.Message}");
                    skipped++;
                }
            }
            
            LogEx.Log($"created: {created}, skipped: {skipped}");
            return relics;
        }

        private static Dictionary<string, RelicEffectBase> MakeEffectMap()
        {
            // DB 검증
            var db = CardevilCore.Instance.Database.Database;
            var list = db.RelicEffectOnEvaluationDataList;
            if (list == null || list.Count == 0)
            {
                LogEx.LogError("RelicEffect OnEvaluation Data가 존재하지 않음");
                return new Dictionary<string, RelicEffectBase>();
            }

            var effectDict = new Dictionary<string, RelicEffectBase>();
            foreach (var d in list)
            {
                // 데이터 검증
                if (d == null)
                {
                    LogEx.LogWarning($"effect data is null. Skip {d.EffectId}.");
                    continue;
                }
                
                // Data -> RelicEffectSpec으로 역직렬화
                var spec = DataToSpec(d);
                
                // 생성 및 추가
                try
                {
                    var effect = Create(spec);
                    effectDict[spec.EffectId] = effect;
                }
                catch(Exception ex)
                {
                    LogEx.LogWarning($"Failed to create relic effect. (Id: {spec.EffectId}) : {ex.Message}");
                }

                continue;
                RelicEffectSpec DataToSpec(RelicEffectOnEvaluationData data)
                {
                    return new RelicEffectSpec
                    {
                        EffectId = data.EffectId.Trim('"', ' '),
                        EffectType = data.EffectType,
                        IsPermanent = data.IsPermenent,
                        ExecutionCount = data.ExecutionCount,
                
                        TriggerHandRanking = data.TriggerHandRanking,
                        TriggerHp = data.TriggerHp,
                        TriggerPossibility = data.TriggerPossibility,
                
                        IsBasedKillCount = data.IsBasedKillCount,
                        IsPlus = data.IsPlus,
                        EffectValue = data.EffectValue
                    };
                }
                RelicEffectBase Create(RelicEffectSpec s)
                {
                    if (!EffectFactoryMap.TryGetValue(s.EffectType, out var f))
                        throw new InvalidDataException($"the effect factory of {s.EffectType} type is not registered");

                    return f.Create(s);
                }
            }

            return effectDict;
        }
        
        private static void Register(string effectType, IRelicEffectFactory factory) => EffectFactoryMap[effectType] = factory;
        
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