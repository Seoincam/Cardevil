using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class CustomClassTest    {

        /// <summary> 문자열 </summary>
        public string stringVar;
        /// <summary> 숫자 </summary>
        public int integerVar;
        /// <summary> 풀네임 enum리스트 </summary>
        public List<Define.SlotRewardType> fullEnumList;
        /// <summary> (Reference:Enum<SlotRewardType>) 
         /// 짧은 이름 enum 리스트 </summary>
        public string shortEnumList;
        /// <summary> (Reference:Class<DBSampleEntryClassJson>) 
         /// 커스텀 클래스 </summary>
        public string CustomClass;
        /// <summary> (Reference:List<Class<DBSampleEntryClassJson>>) 
         /// 커스텀 클래스 리스트 </summary>
        public string CustomClassList;
        /// <summary> (Reference:List<List<int>>) 
         /// 2차원 리스트 </summary>
        public string ListList;
    }
}
