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
    public partial class TooltipData: IDBData
    {

        /// <summary> 툴팁 구분자 </summary>
        public string identifier;
        public string titleTextID;
        public string decriptionTextID;
    }

}
