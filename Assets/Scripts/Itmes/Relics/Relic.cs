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



        public string RelicId => _data.RelicId;

        public string Name => _data.DisplayName;

        public string Description => _data.DisplayDescription;

        public IReadOnlyList<RelicEffect> Effects => _effects;



        public Relic(RelicDataManager manager, RelicData data)
        {
            _data = data;

            if (_data.EffectIds != null)
            {
                foreach (var effectId in _data.EffectIds)
                    _effects.Add(manager.GetEffect(effectId));
            }
            else
            {
                Debug.LogWarning($"Relic '{_data?.RelicId}' has null EffectIds.");
            }
        }
    }
}
