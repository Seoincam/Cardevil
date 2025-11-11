using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class MachineReward    {

        /// <summary> 이름 </summary>
        public Define.SlotRewardType ItemName;
        /// <summary> 희귀도 </summary>
        public Define.RareType Rank;
        /// <summary> 가치 </summary>
        public int Value;
        /// <summary> 특수 값 ( RandomGold에서는 최소값을 넣어주세요 ) </summary>
        public string Comment;
        /// <summary> 아이템 가중치 </summary>
        public int ItemProbability;
        /// <summary> 숫자 가중치 </summary>
        public List<int> RandomProbablility;
        /// <summary> 아이템이미지 </summary>
        public string URL;
    }
}
