using Database.Generated;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Cardevil.Attributes;
using UnityEngine.Serialization;

namespace Cardevil.Relics
{
    [Serializable]
    public class Relic
    {
        [SerializeField, VisibleOnly] private string id;
        [SerializeField, VisibleOnly] private int level;
        [SerializeField, VisibleOnly] private string displayName;
        [SerializeField, VisibleOnly] private string displayDescription;
        [SerializeReference, VisibleOnly] private List<RelicEffectBase> effects;

        public string Id => id;
        public int Level => level;
        public string DisplayName => displayName;
        public string DisplayDescription => displayDescription;
        public IReadOnlyList<RelicEffectBase> Effects => effects;

        public Relic(string id, int level, string displayName, string displayDescription, List<RelicEffectBase> effects)
        {
            this.id = id;
            this.level = level;
            this.displayName = displayName;
            this.displayDescription = displayDescription;
            this.effects = effects;
        }
    }
}
