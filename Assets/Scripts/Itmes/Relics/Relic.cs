using Database.Generated;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Cardevil.Attributes;

namespace Cardevil.Relics
{
    [Serializable]
    public class Relic
    {
        [SerializeField, VisibleOnly] string _id;
        [SerializeField, VisibleOnly] int _level;
        [SerializeField, VisibleOnly] string _displayName;
        [SerializeField, VisibleOnly] string _displayDescription;
        List<RelicEffectBase> _effects = new();


        public string Id => _id;
        public int Level => _level;
        public string DisplayName => _displayName;
        public string DisplayDescription => _displayDescription;
        public IReadOnlyList<RelicEffectBase> Effects => _effects;



        public Relic(RelicManager manager, RelicData data)
        {
            _id = data.RelicId;
            _displayName = data.DisplayName;
            _displayDescription = data.DisplayDescription;
            _level = data.Level;

            if (data.EffectIds != null)
            {
                foreach (var effectId in data.EffectIds)
                {
                    var effect = manager.GetEffectById(effectId);
                    effect.Init(this);
                    _effects.Add(effect);
                }
            }
            else
            {
                Debug.LogWarning($"Relic '{data?.RelicId}' has null EffectIds.");
            }
        }
    }
}
