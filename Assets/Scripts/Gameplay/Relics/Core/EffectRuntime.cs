namespace Cardevil.Gameplay.Relics.Core
{
    public abstract class EffectRuntime
    {
        protected readonly RelicInstance Context;

        protected EffectRuntime(RelicInstance context)
        {
            Context = context;
        }

        /// <summary>
        /// 활성화 상태로 전활될 때 호출.
        /// </summary>
        public abstract void OnActive();

        /// <summary>
        /// 활성화 상태에서 비활성화 상태로 전활될 때 호출.
        /// </summary>
        public abstract void OnInactive();
        


        public virtual object CaptureState() => null;
        public virtual void RestoreState(object state) { }
    }
}