using Cardevil.Attributes;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.InStage;
using Cardevil.Core;
using Cardevil.DataStructure;
using Cardevil.Utils;
using System;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardLibrary
    {
        /// <summary>
        /// 읽기만 가능한 파이프라인을 반환.
        /// </summary>
        IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id);
    }
    
    [Serializable]
    public class CardLibrary : IClearable, IReadOnlyCardLibrary
    {
        private EnhancementDataLibrary _enhancementDataLibrary;
        
        // <id, data>
        [SerializeField, VisibleOnly] private SerializableDict<int, CardDataPipeline> pipelineMap = new();
        
        /*
         * 파이프라인을 바탕으로 생성된 데이터들.
         * 파이프라인에 수정이 있을 때, 해당 Id의 데이터만 갱신함.
         * TODO: 갱신 로직 추가해야함.
         */
        [SerializeField, VisibleOnly] private SerializableDict<int, CardData> dataMap = new();
        // TODO: Visual도 추가
        
        public void Init(EnhancementDataLibrary enhancementDataLibrary)
        {
            _enhancementDataLibrary = enhancementDataLibrary;
            
            Clear();
        }
        
        /// <summary>
        /// <see cref="CardDataPipeline"/>을 로드하거나, 기존 세이브가 없을 경우 새로 생성.
        /// <see cref="CardDataPipeline"/>을 바탕으로 <see cref="CardData"/>를 생성.
        /// 주의! 현재 로드 로직 없음.
        /// </summary>
        public void CreateBasePipelines()
        {
            // TODO: 세이브파일 로드 체크 로직 넣어야함.
            
            pipelineMap.CreateBasePipelines(_enhancementDataLibrary);
            
            foreach ((int id, var pipeline) in pipelineMap)
            {
                var cardData = pipeline.Build();
                if (cardData != null)
                    dataMap[id] = cardData;
            }
        }

        public void Clear()
        {
            pipelineMap.Clear();
            dataMap.Clear();
        }
        
        public CardDataPipeline GetPipelineById(int id)
        {
            if (id < 0 || id > 49)
            {
                LogEx.LogError("잘못된 id를 입력했습니다.");
                return null;
            }

            if (!pipelineMap.TryGetValue(id, out var pipeline))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return pipeline;
        }

        public CardData GetDataById(int id)
        {
            if (id < 0 || id > 49)
            {
                LogEx.LogError("잘못된 id를 입력했습니다.");
                return null;
            }
            
            if (!dataMap.TryGetValue(id, out var data))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }
            
            return data;
        }

        /// <summary>
        /// <see cref="CardDataPipeline"/>을 기반으로
        /// <see cref="CardData"/>를 갱신합니다.
        /// </summary>
        /// <param name="id"></param>
        public void UpdateData(int id)
        {
            var data = pipelineMap[id].Build();
            dataMap[id] = data;
            // TODO: 추후 visual 추가하면 visual도 갱신
        }

        #region IReadOnlyCardLibrary

        public IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id)
        {
            return GetPipelineById(id);
        }

        #endregion
        
    }
}