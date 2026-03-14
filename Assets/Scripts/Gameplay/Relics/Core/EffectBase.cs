namespace Cardevil.Gameplay.Relics.Core
{
    public abstract class EffectBase
    {
        protected IRelicContext Context;
        
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
}