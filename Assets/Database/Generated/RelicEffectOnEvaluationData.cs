using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class RelicEffectOnEvaluationData    {

        /// <summary> inspector상 설명 </summary>
        public string Description;
        /// <summary> ID </summary>
        public string EffectId;
        /// <summary> 확률 (0~1) </summary>
        public float Possibility;
        /// <summary> 발동 Hp </summary>
        public int TriggerHp;
        /// <summary> (Reference:Enum<Cardevil.Relics.EffectExcute>) 
         /// 실행 타입 </summary>
        public string ExecuteType;
        /// <summary> (Reference:Enum<Cardevil.Cards.Evaluations.HandRanking>) 
         /// 발동 족보 (None은 모두) </summary>
        public string TriggerRanking;
        /// <summary> (Reference:Enum<Cardevil.Relics.EffectEvaluation>) 
         /// 효과 종류 </summary>
        public string EvaluationType;
        /// <summary> 효과 값 </summary>
        public float EffectValue;
        /// <summary> (Next) 실행 횟수 </summary>
        public int ExecutionCount;
        /// <summary> (Reference:List<Enum<Cardevil.Cards.Evaluations.HandRanking>>) 
         /// (Next/Permanent) 타겟 족보 리스트 </summary>
        public string TargetRankings;
    }
}
