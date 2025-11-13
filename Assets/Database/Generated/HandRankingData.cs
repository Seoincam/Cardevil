using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class HandRankingData: IDBData {

        /// <summary> 족보 </summary>
        public Cardevil.Cards.Data.HandRanking Ranking;
        /// <summary> 보너스 점수 </summary>
        public int Value;
        /// <summary> 게임상 이름 </summary>
        public string DisplayName;
        /// <summary> 게임상 조건 설명 </summary>
        public string DisplayCondition;
        /// <summary> 게임상 족보 설명창에 뜨는 카드 조합 </summary>
        public List<string> DisplayCards;
        /// <summary> 악마의 계약 </summary>
        public string DisplayEffect;
    }
}
