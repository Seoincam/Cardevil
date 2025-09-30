using Database.Generated;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public class Relic
    {
        [SerializeField] RelicData _data;
        [SerializeField] List<RelicEffect> _effects = new();



        public string Id => _data.RelicId;
        public int Level => _data.Level;

        public string Name => _data.DisplayName;
        public string Description => _data.DisplayDescription;

        public IReadOnlyList<RelicEffect> Effects => _effects;



        public Relic(RelicDataManager manager, RelicData data)
        {
            _data = data;

            if (_data.EffectIds != null)
            {
                foreach (var effectId in _data.EffectIds)
                {
                    var effect = manager.GetEffectById(effectId);
                    effect.Init(this);
                    _effects.Add(effect);
                }

            }
            else
            {
                Debug.LogWarning($"Relic '{_data?.RelicId}' has null EffectIds.");
            }
        }
    }
}
