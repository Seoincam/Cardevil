using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class shop    {

        /// <summary> 이름 </summary>
        public string itemName;
        /// <summary> 희귀도 </summary>
        public Define.RareType rare;
        /// <summary> 가치 </summary>
        public int value;
        /// <summary> 설명 </summary>
        public string coment;
        /// <summary> 아이템이미지 </summary>
        public string URL;
    }
}
