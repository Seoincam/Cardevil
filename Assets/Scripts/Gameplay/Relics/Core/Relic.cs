using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    public enum RelicRarity
    {
        Default,
        MiddleBoss,
        FinalBoss
    }
    
    [Serializable]
    public class Relic
    {
        [Header("Database")]
        [SerializeField] private string id;

        [Header("Setting")] 
        [SerializeField] private RelicRarity rarity;
        
        [Header("Display")]
        [SerializeField] private Sprite displayIcon;
        [SerializeField] private string displayName;
        [SerializeField] private string displayDescription;
        
        [Header("Effects")]
        [SerializeReference] private List<EffectBase> effects = new();

        public Relic(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }
        
        public string Id => id;
        public RelicRarity Rarity => rarity;
        public Sprite DisplayIcon => displayIcon;
        public string DisplayName => displayName;
        public string DisplayDescription => displayDescription;
        public IReadOnlyList<EffectBase> Effects => effects; 
    }
}