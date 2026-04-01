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
    public class RelicDefinition
    {
        [Header("Database")]
        [SerializeField] private string id;

#if UNITY_EDITOR
        [SerializeField] private string commentForEditor;
        public string CommentForEditor => commentForEditor;
#endif

        [Header("Setting")] 
        [SerializeField] private RelicRarity rarity;
        
        [Header("Display")]
        [SerializeField] private Sprite displayIcon;
        [SerializeField] private string displayName;
        [SerializeField] private string displayDescription;
        
        [Header("Effects")]
        [SerializeReference] private List<EffectDefinition> effects = new();

        public RelicDefinition(string id, string displayName)
        {
            this.id = id;
            this.displayName = displayName;
        }

#if UNITY_EDITOR
        public RelicDefinition(
            string id, 
            RelicRarity rarity, 
            string displayName, 
            string displayDescription,
            string commentForEditor)
        {
            this.id = id;
            this.rarity = rarity;
            this.displayName = displayName;
            this.displayDescription = displayDescription;
            this.commentForEditor = commentForEditor;
        }
#endif
        
        public string Id => id;
        public RelicRarity Rarity => rarity;
        public Sprite DisplayIcon => displayIcon;
        public string DisplayName => displayName;
        public string DisplayDescription => displayDescription;
        public IReadOnlyList<EffectDefinition> Effects => effects;
    }
}