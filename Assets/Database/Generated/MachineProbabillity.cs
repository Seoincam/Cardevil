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
        public List<int> RankWeight;
        /// <summary> 다음 레벨까지의 레벨업 비용 </summary>
        public int LevelUpCost;
    }
}
