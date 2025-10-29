using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public class RelicEffectOnEvaluationData    {

        /// <summary> NOEX_설명 </summary>
        public string Description;
        /// <summary> ID </summary>
        public string EffectId;
        /// <summary> 종류 </summary>
        public string EffectType;
        /// <summary> 영구적인가? </summary>
        public bool IsPermenent;
        /// <summary> 실행 횟수 </summary>
        public int ExecutionCount;
        /// <summary> 발동 족보 </summary>
        public Cardevil.Cards.Data.HandRanking TriggerHandRanking;
        /// <summary> 발동 Hp </summary>
        public int TriggerHp;
        /// <summary> 발동 확률 </summary>
        public float TriggerPossibility;
        /// <summary> 적 처치 숫자에 기반? </summary>
        public bool IsBasedKillCount;
        /// <summary> 합인가? </summary>
        public bool IsPlus;
        /// <summary> 효과 값 </summary>
        public float EffectValue;
    }
}
