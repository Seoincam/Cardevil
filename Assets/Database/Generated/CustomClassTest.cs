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
    public partial class CustomClassTest: IDBData
    {

        /// <summary> 문자열 </summary>
        public string stringVar;
        /// <summary> 숫자 </summary>
        public int integerVar;
        /// <summary> 풀네임 enum리스트 </summary>
        public List<Cardevil.Core.Utils.Define.SlotRewardType> fullEnumList;
        /// <summary> 짧은 이름 enum 리스트 </summary>
        public Cardevil.Core.Utils.Define.SlotRewardType shortEnumList;
        /// <summary> 커스텀 클래스 </summary>
        public Database.DBSampleEntryClassJson CustomClass;
        /// <summary> 커스텀 클래스 리스트 </summary>
        public List<Database.DBSampleEntryClassJson> CustomClassList;
        /// <summary> 2차원 리스트 </summary>
        public List<List<int>> ListList;
        /// <summary> 튜플 </summary>
        [NonSerialized]
        [JsonIgnore]
        public (int, int, string) tuple;
        /// <summary> 튜플리스트 </summary>
        [NonSerialized]
        [JsonIgnore]
        public List<(int, int, string)> tupleList;
    }

    public partial class CustomClassTest : ISerializationCallbackReceiver
    {

        [SerializeField, JsonProperty("tuple"), InspectorName("tuple")]
        private DatabaseTuple<int, int, string> tupleSerialized;

        [SerializeField, JsonProperty("tupleList"), InspectorName("tupleList")]
        private List<DatabaseTuple<int, int, string>> tupleListSerialized = new List<DatabaseTuple<int, int, string>>();

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
            tupleSerialized = DatabaseTupleConvert.ToBacking<DatabaseTuple<int, int, string>>(tuple);
            tupleListSerialized = tupleList?.ConvertAll(item => DatabaseTupleConvert.ToBacking<DatabaseTuple<int, int, string>>(item)) ?? new List<DatabaseTuple<int, int, string>>();
        }

        private void SyncTupleFieldsFromSerializedBacking()
        {
            tuple = DatabaseTupleConvert.ToValueTuple<(int, int, string)>(tupleSerialized);
            tupleList = tupleListSerialized?.ConvertAll(item => DatabaseTupleConvert.ToValueTuple<(int, int, string)>(item)) ?? new List<(int, int, string)>();
        }
    }

}
