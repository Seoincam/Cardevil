using Cardevil.Attributes;
using Cardevil.Cards.Core;
using System;
using UnityEngine;

namespace Cardevil.Cards.Enhancements
{
    [Serializable]
    public class EnhancementData
    {
        public Guid Id { get; }
        
        [SerializeField, VisibleOnly] private ModifierType type;
        [SerializeField, VisibleOnly] private int level;
        [SerializeField, VisibleOnly] private int modifierCount;
        
        [SerializeField, VisibleOnly] private int marketCost;
        [SerializeField, VisibleOnly] private int blackMarketCost;
        
        [SerializeField, VisibleOnly] private float successRate;
        [SerializeField, VisibleOnly] private float failRate;
        
        [SerializeField, VisibleOnly] private string description;

        #region getter

        public ModifierType Type => type;
        public int Level => level;
        public int ModifierCount => modifierCount;
        
        public int MarketCost => marketCost;
        public int BlackMarketCost => blackMarketCost;
        
        public float SuccessRate => successRate;
        public float FailRate => failRate;
        
        public string Description => description;

        #endregion
        
        public EnhancementData(ModifierType type, int level, int modifierCount, int marketCost, int blackMarketCost, 
            float successRate, float failRate, string description)
        {
            Id = Guid.NewGuid();
            
            this.type = type;
            this.level = level;
            this.modifierCount = modifierCount;
            
            this.marketCost = marketCost;
            this.blackMarketCost = blackMarketCost;
            
            this.successRate = successRate;
            this.failRate = failRate;
            
            this.description = description;
        }
    }
}