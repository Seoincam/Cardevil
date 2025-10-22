using Cardevil.Cards.Data.Modifiers;
using System;

namespace Cardevil.Cards.Data.Enhancement
{
    [Serializable]
    public struct EnhancementData
    {
        public readonly ModifierType type;
        public readonly int level;
        public readonly int modifierCount;
        
        public readonly int cost;
        public readonly float successRate;
        public readonly float failRate;

        public EnhancementData(ModifierType type, int level, int modifierCount, int cost, float successRate,
            float failRate)
        {
            this.type = type;
            this.level = level;
            this.modifierCount = modifierCount;
            
            this.cost = cost;
            this.successRate = successRate;
            this.failRate = failRate;
        }
    }
}