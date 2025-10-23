using Cardevil.Attributes;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.ScriptableObjects;
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
        IReadOnlyDictionary<int, CardData> DataMap { get; }
        IReadOnlyDictionary<int, CardVisualSpriteSet> VisualSpriteSetMap { get; }
        
        /// <summary>
        /// 읽기만 가능한 파이프라인을 반환.
        /// </summary>
        IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id);
    }
    
    [Serializable]
    public class CardLibrary : IClearable, IReadOnlyCardLibrary
    {
        // <id, data>
        [SerializeField, VisibleOnly] private SerializableDict<int, CardDataPipeline> pipelineMap = new();
        
        // 파이프라인을 바탕으로 생성된 데이터들.
        // 파이프라인에 수정이 있을 때, 해당 Id의 데이터만 갱신함.
        [SerializeField, VisibleOnly] private SerializableDict<int, CardData> dataMap = new();
        [SerializeField, VisibleOnly] private SerializableDict<int, CardVisualSpriteSet> visualSpriteSetMap = new();   
        
        private EnhancementDataLibrary _enhancementDataLibrary;
        private CardVisualSpriteFactorySO _visualSpriteFactory;
        
        public void Init(EnhancementDataLibrary enhancementDataLibrary)
        {
            _enhancementDataLibrary = enhancementDataLibrary;
            
            _visualSpriteFactory = Resources.Load<CardVisualSpriteFactorySO>("ScriptableObjects/Cards/CardVisualSpritesFactory");
            if (!_visualSpriteFactory)
            {
                LogEx.LogError("No CardVisualSpriteFactorySO found");
                return;
            }
            
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
            
            foreach (int id in pipelineMap.Keys)
                UpdateMaps(id);
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
        public void UpdateMaps(int id)
        {
            // Card Data
            var cardData = pipelineMap[id].Build();
            if (cardData != null)
                dataMap[id] = cardData;
            
            // Card Visual Sprite Set
            var spriteSet = dataMap[id].MakeSpriteSet(_visualSpriteFactory);
            if (spriteSet != null)
                visualSpriteSetMap[id] = spriteSet;
        }

        #region IReadOnlyCardLibrary

        public IReadOnlyDictionary<int, CardData> DataMap => dataMap;
        public IReadOnlyDictionary<int, CardVisualSpriteSet> VisualSpriteSetMap => visualSpriteSetMap;

        public IReadOnlyCardDataPipeline GetReadOnlyPipelineById(int id) => GetPipelineById(id);

        #endregion
    }
}