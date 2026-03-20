using Cardevil.UI.GlobalNavigationBar;
using System.Collections.Generic;

namespace Cardevil.Gameplay.Relics.Core
{
    public class RelicInstance
    {
        public RelicDefinition Data { get; }
        public IRelicCommonContext CommonContext { get; }
        
        private readonly List<EffectRuntime> _runtimeEffects = new();

        private RelicIcon _iconInstance;

        public RelicInstance(RelicDefinition data, IRelicCommonContext commonContext)
        {
            Data = data;
            CommonContext = commonContext;
            
            foreach (var def in data.Effects)
            {
                _runtimeEffects.Add(def.CreateRuntimeInstance(this));
            }
        }

        public void Activate()
        {
            foreach (var runtime in _runtimeEffects)
                runtime.OnActive();
        }

        public void Deactivate()
        {
            foreach (var runtime in _runtimeEffects)
                runtime.OnInactive();
        }
        
        public void SetIcon(RelicIcon iconInstance) => _iconInstance = iconInstance;
    }
}