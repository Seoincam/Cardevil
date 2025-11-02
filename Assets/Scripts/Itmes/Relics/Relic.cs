using System.Collections.Generic;
using System;
using UnityEngine;
using Cardevil.Attributes;

namespace Cardevil.Relics
{
    [Serializable]
    public class Relic
    {
        [SerializeField, VisibleOnly] private string id;
        [SerializeField, VisibleOnly] private int level;
        [SerializeField, VisibleOnly] private RelicRarity rarity;
        [SerializeField, VisibleOnly] private string displayName;
        [SerializeField, VisibleOnly] private string displayDescription;
        [SerializeReference, VisibleOnly] private List<RelicEffectBase> effects;

        public string Id => id;
        public int Level => level;
        public RelicRarity Rarity => rarity;
        public string DisplayName => displayName;
        public string DisplayDescription => displayDescription;
        public IReadOnlyList<RelicEffectBase> Effects => effects;

        public Relic(string id, int level, RelicRarity rarity, string displayName, string displayDescription, List<RelicEffectBase> effects)
        {
            this.id = id;
            this.level = level;
            this.rarity = rarity;
            this.displayName = displayName;
            this.displayDescription = displayDescription;
            this.effects = effects;
        }
    }
}
