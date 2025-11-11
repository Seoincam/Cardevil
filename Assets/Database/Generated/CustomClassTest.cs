using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class CustomClassTest    {

        /// <summary> 문자열 </summary>
        public string stringVar;
        /// <summary> 숫자 </summary>
        public int integerVar;
        /// <summary> 풀네임 enum리스트 </summary>
        public List<Define.SlotRewardType> fullEnumList;
        /// <summary> 짧은 이름 enum 리스트 </summary>
        public Define.SlotRewardType shortEnumList;
        /// <summary> 커스텀 클래스 </summary>
        public Cardevil.Database.Sample.DBSampleEntryClassJson HPRate;
        /// <summary> (Reference:Class<DBSampleEntryClassTarget>) 
         /// 커스텀 클래스(외부구현) </summary>
        public string HPRateProbabillity;
    }
}
