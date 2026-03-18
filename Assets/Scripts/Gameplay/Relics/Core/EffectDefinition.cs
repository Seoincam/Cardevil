using System;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public abstract class EffectDefinition
    {
        /// <summary>
        /// 유물 에디터 상단에 표시될 이름.
        /// </summary>
        public virtual string EditorName => GetType().Name;
        
        /// <summary>
        /// 유물 에디터에 표시될 자동 요약 설명.
        /// </summary>
        public virtual string EditorDescription => "설명이 등록되지 않은 이펙트입니다.";
        
        public abstract EffectRuntime CreateRuntimeInstance(IRelicContext context);
    }

    public abstract class EffectRuntime
    {
        protected readonly IRelicContext Context;

        protected EffectRuntime(IRelicContext context)
        {
            Context = context;
        }
        
        /// <summary>
        /// 활성화 상태에서 비활성화 상태로 전활될 때 호출.
        /// </summary>
        public virtual void OnInactive() { }
        
        /// <summary>
        /// 비활성화 상태에서 활성화 상태로 전활될 때 호출.
        /// </summary>
        public virtual void OnActive() { }

        public virtual object CaptureState() => null;
        public virtual void RestoreState(object state) { }
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