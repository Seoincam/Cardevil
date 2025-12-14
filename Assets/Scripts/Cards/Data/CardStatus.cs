using Cardevil.Attributes;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Save;
using Cardevil.Core;
using Cardevil.DataStructure.Serializables;
using Cardevil.Save;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data
{
    public interface IReadOnlyCardStatus 
    {
        /// <summary>
        /// Pipeline, data, visualSprite Set의 개수.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// 해당 id의 <see cref="CardData"/>를 반환.
        /// </summary>
        CardData GetCardDataById(int id);
        
        /// <summary>
        /// 해당 id의 <see cref="IReadOnlyCardDataPipeline"/>을 반환.
        /// </summary>
        IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id);
    }

    /// <summary>
    /// 카드 상태 관리.
    /// 카드 파이프라인 및 빌드된 카드 데이터 관리, 세이브/로드 및 신규 게임 초기화 처리.
    /// </summary>
    [Serializable]
    public class CardStatus : IClearable, IReadOnlyCardStatus, ISaveLoad, INewGameInitializable
    {
        [Tooltip("카드 ID별 데이터 파이프라인 맵."), VisibleOnly]
        public SerializableDictionary<int, CardDataPipeline> pipelineMap = new();

        // 파이프라인을 바탕으로 생성된 데이터들.
        // 파이프라인에 수정이 있을 때, 해당 Id의 데이터만 갱신함.
        [SerializeField, VisibleOnly] private SerializableDictionary<int, CardData> dataMap = new();

        private EnhancementDataLibrary _enhancementDataLibrary;

        public CardStatus(EnhancementDataLibrary enhancementDataLibrary)
        {
            _enhancementDataLibrary = enhancementDataLibrary;
        }

        public void Clear()
        {
            pipelineMap.Clear();
            dataMap.Clear();
        }

        public CardDataPipeline GetPipelineById(int id)
        {
            if (!ValidateId(id))
                return null;

            if (!pipelineMap.TryGetValue(id, out var pipeline))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return pipeline;
        }

        public CardData GetDataById(int id)
        {
            if (!ValidateId(id))
                return null;

            if (!dataMap.TryGetValue(id, out var data))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return data;
        }

        /// <summary>
        /// <see cref="CardDataPipeline"/>을 기반으로
        /// <see cref="CardData"/>를 재빌드합니다.
        /// </summary>
        public void UpdateDataMap(int id)
        {
            var cardData = pipelineMap[id].Build();
            if (cardData != null)
                dataMap[id] = cardData;
        }

        private bool ValidateId(int id)
        {
            if (id < 0 || id > 49)
            {
                LogEx.LogError($"Invalid Id : {id}");
                return false;
            }

            return true;
        }

        #region IReadOnlyCardStatus

        public int Count
        {
            get
            {
                if (pipelineMap.Count != dataMap.Count)
                {
                    LogEx.LogError("Incorrect number of maps!");
                    return 0;
                }

                return pipelineMap.Count;
            }
        }

        public CardData GetCardDataById(int id)
        {
            if (!ValidateId(id))
                return null;

            if (!dataMap.TryGetValue(id, out var data))
            {
                LogEx.LogError($"Cannot find CardData. Id: {id}");
                return null;
            }

            return data;
        }

        public IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id) => GetPipelineById(id);

        #endregion
        
        public void SetUpNewGame(GameSave currentSave)
        {
            this.CreateBasePipelines(_enhancementDataLibrary);

            foreach (int id in pipelineMap.Keys)
                UpdateDataMap(id);
        }
        
        public void Save(GameSave currentSave)
        {
            var saveData = new CardStatusSaveData { pipelines = new List<CardDataPipelineSaveData>() };

            foreach (var pipeline in pipelineMap.Values)
                saveData.pipelines.Add(pipeline.Serialize());

            currentSave.cardStatusData = saveData;
        }

        public void Load(GameSave currentSave)
        {
            Clear();

            var saveData = currentSave.cardStatusData;
            if (saveData?.pipelines == null)
                return;

            foreach (var p in saveData.pipelines)
            {
                var pipeline = CardDataPipeline.FromSaveData(p);
                pipelineMap[p.id] = pipeline;
                UpdateDataMap(p.id);
                
            }
        }
    }
}