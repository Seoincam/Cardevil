using Cardevil.Relics;
using Cardevil.Utils;
using Database.Generated;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Cardevil.Items.Relics.Factory
{
    [Serializable]
    public class RelicFactory
    {
        [SerializeReference] private List<RelicEffectBase> effects;

        public void MakeEffectInstances()
        {
            var db = Managers.Database.Database;
            var list = db.RelicEffectOnEvaluationDataList;
            if (list == null || list.Count == 0)
            {
                LogEx.LogError("RelicEffect OnEvaluation Data가 존재하지 않음");
                effects = new();
                return;
            }

            var results = new List<RelicEffectBase>(list.Count);
            foreach (var data in list)
            {
                // Data -> RelicEffectSpec으로 역직렬화
                var spec = DataToSpec(data);

                // 검증

                // 생성 및 추가
                try
                {
                    var effect = RelicEffectRegistry.Create(spec);
                    results.Add(effect);
                }
                catch
                {
                    LogEx.LogError($"Failed to create relic effect. (Id: {spec.EffectId})");
                }
            }

            effects = results;
        }

        private static RelicEffectSpec DataToSpec(RelicEffectOnEvaluationData data)
        {
            return new RelicEffectSpec
            {
                EffectId = data.EffectId,
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
    }
}