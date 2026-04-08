using Cardevil.Core.Attributes;
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
        [SerializeField, VisibleOnly] private string id;

#if UNITY_EDITOR
        [SerializeField, VisibleOnly] private string commentForEditor;
        public string CommentForEditor => commentForEditor;
#endif

        [Header("Setting")] 
        [SerializeField, VisibleOnly] private RelicRarity rarity;
        
        [Header("Display")]
        [SerializeField, VisibleOnly] private Sprite displayIcon;
        [SerializeField, VisibleOnly] private string displayName;
        [SerializeField, VisibleOnly] private string displayDescription;
        
        [Header("Effects")]
        [SerializeReference, VisibleOnly] private List<EffectDefinition> effects = new();

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