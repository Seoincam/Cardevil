using System;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public abstract class EffectBase
    {
        protected IRelicContext Context;
        
        protected EffectBase() { }
        protected EffectBase(IRelicContext context)
        {
            Context = context;    
        }
        
        /// <summary>
        /// 유물이 활성 상태에서 비활성 상태로 전환될 때 호출됨.
        /// </summary>
        public virtual void OnInactive() { }
        
        /// <summary>
        /// 유물이 비활성 상태에서 활성 상태로 전환될 때 호출됨.
        /// </summary>
        public virtual void OnActive() { }
    }
    
    /// <summary>
    /// 플레이어의 스탯을 변화시키는 객체가 구현해야함.
    /// </summary>
    public interface IStatModifier
    {
        int ModifierId { get; set; }
        PlayerStatType TargetType { get; }
        int Modify(int previousValue);
    }

    /// <summary>
    /// 특정 이벤트에 반응하는 객체가 구현해야함.
    /// </summary>
    public interface IGameEventListener
    {
        
    }
}