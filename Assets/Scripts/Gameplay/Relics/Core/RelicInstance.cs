using Cardevil.UI.GlobalNavigationBar;
using System.Collections.Generic;

namespace Cardevil.Gameplay.Relics.Core
{
    public class RelicInstance
    {
        public RelicDefinition Data { get; }
        public IRelicCommonContext CommonContext { get; }
        
        private readonly List<EffectInstance> _runtimeEffects = new();

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

        public RelicSaveData CaptureSaveData()
        {
            var saveData = new RelicSaveData { relicId = Data.Id };

            for (int i = 0; i < _runtimeEffects.Count; i++)
            {
                var state = _runtimeEffects[i].CaptureState();
                if (state != null)
                {
                    saveData.effectStates.Add(new EffectSaveData
                    {
                        effectIndex = i,
                        saveData = state.ToString()
                    });
                }
            }

            return saveData;
        }

        public void RestoreSaveData(RelicSaveData saveData)
        {
            foreach (var effectSave in saveData.effectStates)
            {
                if (effectSave.effectIndex < _runtimeEffects.Count)
                {
                    _runtimeEffects[effectSave.effectIndex].RestoreState(effectSave.saveData);
                }
            }
        }
    }
}