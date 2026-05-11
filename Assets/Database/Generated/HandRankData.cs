using System.Text;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class HandRankData: IDBData
    {

        /// <summary> 족보 </summary>
        public Cardevil.Card.Common.Core.HandRank Ranking;
        /// <summary> 보너스 점수 </summary>
        public int Value;
        /// <summary> 게임상 이름 </summary>
        public string DisplayName;
        /// <summary> 게임상 조건 설명 </summary>
        public string DisplayCondition;
        /// <summary> 게임상 족보 설명창에 뜨는 카드 조합 </summary>
        [NonSerialized]
        [JsonIgnore]
        public List<(Cardevil.Card.Common.Core.CardColor, int, bool)> DisplayCards;
    }

    public partial class HandRankData : ISerializationCallbackReceiver
    {

        [SerializeField, JsonProperty("DisplayCards"), InspectorName("DisplayCards")]
        private List<DatabaseTuple<Cardevil.Card.Common.Core.CardColor, int, bool>> DisplayCardsSerialized = new List<DatabaseTuple<Cardevil.Card.Common.Core.CardColor, int, bool>>();

        public void OnBeforeSerialize()
        {
            SyncTupleFieldsToSerializedBacking();
        }

        public void OnAfterDeserialize()
        {
            SyncTupleFieldsFromSerializedBacking();
        }

        [OnSerializing]
        private void OnJsonSerializing(StreamingContext context)
        {
            SyncTupleFieldsToSerializedBacking();
        }

        [OnDeserialized]
        private void OnJsonDeserialized(StreamingContext context)
        {
            SyncTupleFieldsFromSerializedBacking();
        }

        private void SyncTupleFieldsToSerializedBacking()
        {
            DisplayCardsSerialized = DisplayCards?.ConvertAll(item => DatabaseTupleConvert.ToBacking<DatabaseTuple<Cardevil.Card.Common.Core.CardColor, int, bool>>(item)) ?? new List<DatabaseTuple<Cardevil.Card.Common.Core.CardColor, int, bool>>();
        }

        private void SyncTupleFieldsFromSerializedBacking()
        {
            DisplayCards = DisplayCardsSerialized?.ConvertAll(item => DatabaseTupleConvert.ToValueTuple<(Cardevil.Card.Common.Core.CardColor, int, bool)>(item)) ?? new List<(Cardevil.Card.Common.Core.CardColor, int, bool)>();
        }
    }

}
