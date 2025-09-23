using Cardevil.Cards;
using UnityEngine;

namespace Cardevil.Relic
{
    /// <summary>
    /// 유물의 효과를 실행하는 SO의 베이스 클래스.
    /// </summary>
    public abstract class RelicEffectSO : ScriptableObject
    {
        public virtual void Apply() { }
        public virtual void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx) { }
    }

    public interface IOnAcquireEffect { void OnAcquire(); }
}