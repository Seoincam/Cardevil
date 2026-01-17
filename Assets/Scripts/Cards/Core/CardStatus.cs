using Cardevil.Attributes;
using Cardevil.Cards.Enhancements;
using Cardevil.Cards.Persistence;
using Cardevil.Cards.Utils;
using Cardevil.Core;
using Cardevil.DataStructure.Serializables;
using Cardevil.Save;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    public interface IReadOnlyCardStatus 
    {
        /// <summary>
        /// Spec, data 개수.
        /// </summary>
        int Count { get; }
        
        /// <summary>
        /// 해당 id의 <see cref="CardData"/>를 반환.
        /// </summary>
        CardData GetCardDataById(int id);
        
        /// <summary>
        /// 해당 id의 <see cref="IReadOnlyCardSpec"/>을 반환.
        /// </summary>
        IReadOnlyCardSpec GetReadOnlySpecById(int id);
    }

    /// <summary>
    /// 카드 상태 관리.
    /// 카드 스펙 및 빌드된 카드 데이터 관리, 세이브/로드 및 신규 게임 초기화 처리.
    /// </summary>
    [Serializable]
    public class CardStatus : IClearable, IReadOnlyCardStatus, ISaveLoad, INewGameInitializable
    {
        [Tooltip("카드 ID별 데이터 스펙 맵."), VisibleOnly]
        public SerializableDictionary<int, CardSpec> specMap = new();

        // 스펙을 바탕으로 생성된 데이터들.
        // 스펙의 수정이 있을 때, 해당 Id의 데이터만 갱신함.
        [SerializeField, VisibleOnly] private SerializableDictionary<int, CardData> dataMap = new();

        private EnhancementDataLibrary _enhancementDataLibrary;

        public static IReadOnlyCardStatus Current;
        
        public CardStatus(EnhancementDataLibrary enhancementDataLibrary)
        {
            _enhancementDataLibrary = enhancementDataLibrary;
            Current = this;
        }

        public void Clear()
        {
            specMap.Clear();
            dataMap.Clear();
        }

        public CardSpec GetSpecById(int id)
        {
            if (!ValidateId(id))
                return null;

            if (!specMap.TryGetValue(id, out var cardSpec))
            {
                LogEx.LogError($"존재하지 않는 id입니다. : {id}");
                return null;
            }

            return cardSpec;
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
        /// <see cref="CardSpec"/>을 기반으로
        /// <see cref="CardData"/>를 재빌드합니다.
        /// </summary>
        public void UpdateDataMap(int id)
        {
            var cardData = specMap[id].Build();
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
                if (specMap.Count != dataMap.Count)
                {
                    LogEx.LogError("Incorrect number of maps!");
                    return 0;
                }

                return specMap.Count;
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

        public IReadOnlyCardSpec GetReadOnlySpecById(int id) => GetSpecById(id);

        #endregion
        
        public void SetUpNewGame(GameSave currentSave)
        {
            this.CreateBaseSpec(_enhancementDataLibrary);

            foreach (int id in specMap.Keys)
                UpdateDataMap(id);
        }
        
        public void Save(GameSave currentSave)
        {
            var saveData = new CardStatusSaveData { specList = new List<CardSpecSaveData>() };

            foreach (var cardSpec in specMap.Values)
                saveData.specList.Add(cardSpec.Serialize());

            currentSave.cardStatusData = saveData;
        }

        public void Load(GameSave currentSave)
        {
            Clear();

            var saveData = currentSave.cardStatusData;
            if (saveData?.specList == null)
                return;

            foreach (var p in saveData.specList)
            {
                var cardSpec = CardSpec.FromSaveData(p);
                specMap[p.id] = cardSpec;
                UpdateDataMap(p.id);
                
            }
        }
    }
}