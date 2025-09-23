using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    /// <summary>
    /// 유물의 조건을 검사하는 SO의 베이스 클래스.
    /// </summary>
    public abstract class RelicConditionSO : ScriptableObject
    {
        public virtual bool CanTrigger() { return false; }
        public virtual bool CanTrigger(CardResultContext resultCtx, CardEvaluationContext evaluationCtx) { return false; }
    }
}

