using System.Collections.Generic;

namespace Cardevil.Gameplay.Relics.Core
{
    public class RelicInstance
    {
        public RelicDefinition Data { get; }
        public IRelicCommonContext CommonContext { get; }
        
        private readonly List<EffectRuntime> _runtimes = new();

        public RelicInstance(RelicDefinition data, IRelicCommonContext commonContext)
        {
            Data = data;
            CommonContext = commonContext;
            
            foreach (var def in data.Effects)
            {
                _runtimes.Add(def.CreateRuntimeInstance(this));
            }
        }

        public void Activate()
        {
            foreach (var runtime in _runtimes)
                runtime.OnActive();
        }

        public void Deactivate()
        {
            foreach (var runtime in _runtimes)
                runtime.OnInactive();
        }
    }
}