using Cardevil.Attributes;
using Cardevil.DataStructure.Serializables;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            if (!providerMap.TryGetValue(provider.ScoreStepType, out List<IScoreProvider> list))
            {
                list = new List<IScoreProvider>();
                providerMap.Add(provider.ScoreStepType, list);
            }
            
            list.Add(provider);

            var id = _nextId++;
            _providerIdMap.Add(id, provider);

            _needSortMap[provider.ScoreStepType] = true;
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
    }
}