using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class RelicOnEvaluation    {

        /// <summary> 식별자 </summary>
        public string id;
        /// <summary> (Reference:Enum<Cardevil.Cards.HandRanking>) 
         /// 발동 트리거 랭킹 (None일 경우 모두) </summary>
        public string triggerRanking;
        /// <summary> (Reference:Enum<Cardevil.Relic.RankingBasedConditionSO.ExecuteType>) 
         /// 실행 타입 </summary>
        public string executeType;
        /// <summary> 발동 확률 (0~1) </summary>
        public float possibility;
        /// <summary> Next일 시, 실행 횟수 </summary>
        public int executionCount;
        /// <summary> (Reference:List<Enum<Cardevil.Cards.HandRanking>>) 
         /// Next, Permanent일 시, 타겟 족보 리스트 </summary>
        public string targetRankings;
        /// <summary> (Reference:Enum<Cardevil.Relic.RankingBasedConditionSO.EffectType>) 
         /// 효과 종류 </summary>
        public string effectType;
        /// <summary> 효과 값 (불필요시 0) </summary>
        public float effectValue;
    }
}
