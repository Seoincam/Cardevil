using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class HandRankData: IDBData {

        /// <summary> 족보 </summary>
        public Cardevil.Card.Common.Core.HandRank Ranking;
        /// <summary> 보너스 점수 </summary>
        public int Value;
        /// <summary> 게임상 이름 </summary>
        public string DisplayName;
        /// <summary> 게임상 조건 설명 </summary>
        public string DisplayCondition;
        /// <summary> 게임상 족보 설명창에 뜨는 카드 조합 (색) </summary>
        public List<Cardevil.Card.Common.Core.CardColor> DisplayCardColors;
        /// <summary> 게임상 족보 설명창에 뜨는 카드 조합 (숫자) </summary>
        public List<int> DisplayCardNumbers;
    }
}
