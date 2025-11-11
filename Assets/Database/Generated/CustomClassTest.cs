using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class CustomClassTest: IDBData {

        /// <summary> 문자열 </summary>
        public string stringVar;
        /// <summary> 숫자 </summary>
        public int integerVar;
        /// <summary> 풀네임 enum리스트 </summary>
        public List<Define.SlotRewardType> fullEnumList;
        /// <summary> 짧은 이름 enum 리스트 </summary>
        public Define.SlotRewardType shortEnumList;
        /// <summary> 커스텀 클래스 </summary>
        public Database.DBSampleEntryClassJson CustomClass;
        /// <summary> 커스텀 클래스 리스트 </summary>
        public List<Database.DBSampleEntryClassJson> CustomClassList;
        /// <summary> 2차원 리스트 </summary>
        public List<ListWrapper<int>> ListList;
    }
}
