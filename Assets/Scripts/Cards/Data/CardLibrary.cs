using Cardevil.Cards.Data.Enhancement;
using Cardevil.Core;
using Cardevil.DataStructure;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardLibrary
    {
        IReadOnlyDictionary<int, CardDataPipeline> Pipelines { get; }
    }
    
    [Serializable]
    public class CardLibrary : IClearable, IReadOnlyCardLibrary
    {
        private EnhancementDataLibrary _enhancementDataLibrary;
        
        // <id, data>
        [SerializeField] private SerializableDict<int, CardDataPipeline> pipelines = new();
        // TODO: Visual도 추가

        #region IReadOnlyCardLibrary

        public IReadOnlyDictionary<int, CardDataPipeline> Pipelines => pipelines;

        #endregion
        
        public void Init(EnhancementDataLibrary enhancementDataLibrary)
        {
            _enhancementDataLibrary = enhancementDataLibrary;
            
            Clear();
            // TODO: 세이브파일 로드 체크 로직 넣어야함.
            pipelines.CreateBasePipelines(_enhancementDataLibrary);
        }

        public void Clear()
        {
            pipelines.Clear();
        }
        
        public CardDataPipeline GetPipelineById(int id)
        {
            if (id < 0 || id > 50)
            {
                LogEx.LogError("잘못된 id를 입력했습니다.");
                return null;
            }

            if (!pipelines.TryGetValue(id, out var pipeline))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return pipeline;
        }

    }
}