using Cardevil.Core.Attributes;
using Cardevil.Core.DataStructure.Serializables;
using Cardevil.Core.Utils;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Test.DebugConsole;
using Cardevil.Core.Bootstrap;

namespace Cardevil.Card.InStage.Score.Step
{
    [Serializable]
    public class ScoreProviderRegistry : IScoreProviderRegistry
    {
        [SerializeReference, VisibleOnly] 
        private SerializableDictionary<ScoreStepType, List<IScoreProvider>> providerMap = new();
        
        private Dictionary<int, IScoreProvider> _providerIdMap = new();
        private int _nextId;

        private Dictionary<ScoreStepType, bool> _needSortMap = new();
        
        public int Register(IScoreProvider provider)
        {
            LogEx.Log(provider.ScoreStepType.ToString());
            
            if (!providerMap.TryGetValue(provider.ScoreStepType, out List<IScoreProvider> list))
            {
                list = new List<IScoreProvider>();
                providerMap.Add(provider.ScoreStepType, list);
            }
            
            list.Add(provider);

            var id = _nextId++;
            _providerIdMap.Add(id, provider);

            _needSortMap[provider.ScoreStepType] = true;
            
            LogEx.Log(providerMap.Count + "개의 ScoreStepType이 존재함.");
            return id;
        }

        public void SafeUnregister(int id, IScoreProvider provider)
        {
            if (!_providerIdMap.TryGetValue(id, out var providerById) ||
                providerById != provider)
            {
                LogEx.LogWarning("id와 일치하는 Provider가 없습니다.");
                return;
            }

            _providerIdMap.Remove(id);
            Unregister(provider);
        }

        public IReadOnlyList<IScoreProvider> GetProviders(ScoreStepType type)
        {
            if (!providerMap.TryGetValue(type, out var providers))
            {
                return null;
            }

            if (_needSortMap.TryGetValue(type, out bool needSort) && needSort)
            {
                // TODO: 순서를 기획 측에서 정한다면 정렬 추가. - @Seoincam
            }
            return providers;
        }

        private void Unregister(IScoreProvider provider)
        {
            providerMap[provider.ScoreStepType].Remove(provider);
        }

        [ConsoleCommand("printScoreProviders", "현재 등록된 모든 ScoreProvider를 출력합니다.")]
        private static void PrintScoreProviders()
        {
            var registry = CardevilCore.Game.ScoreProviderRegistry;
            var sb = new StringBuilder();
            
            foreach (var kvp in registry.providerMap)
            {
                sb.AppendLine($"[{kvp.Key}] 단계 (총 {kvp.Value.Count}개):");
                foreach (var provider in kvp.Value)
                {
                    sb.AppendLine($"  - ID: {provider.Id}, Type: {provider.GetType().Name}");
                }
            }
            
            if (registry.providerMap.Count == 0) sb.AppendLine("등록된 Provider가 없습니다.");
            
            LogEx.Log(sb.ToString());
        }
    }
}