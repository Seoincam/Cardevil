using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class MachineProbabillity: IDBData {

        /// <summary> 머신레벨 </summary>
        public int MachineLevel;
        /// <summary> 랭크가중치 </summary>
        public List<int> RankProbabillity;
    }
}
