using Cardevil.Cards.Data.Modifiers.Move;
using Cardevil.Cards.Data.Modifiers.Number;

namespace Cardevil.Cards.Data
{
    public struct NumberEnhancementData
    {
        public readonly NumberModifierType type;
        public readonly int level;
        
        public readonly int modifierCount;
        public readonly int cost;
        public readonly float successRate;
        public readonly float maintainRate;

        public NumberEnhancementData(NumberModifierType type, int level, int modifierCount, int cost,
            float successRate, float maintainRate)
        {
            this.type = type;
            this.level = level;
            
            this.modifierCount = modifierCount;
            this.cost = cost;
            this.successRate = successRate;
            this.maintainRate = maintainRate;
        }
    }

    public struct MoveEnhancementData
    {
        public readonly MoveModifierType type;
        public readonly int level;
        
        public readonly int modifierCount;
        public readonly int cost;
        public readonly float successRate;
        public readonly float maintainRate;

        public MoveEnhancementData(MoveModifierType type, int level, int modifierCount, int cost, 
            float successRate, float maintainRate)
        {
            this.type = type;
            this.level = level;
            
            this.modifierCount  = modifierCount;
            this.cost = cost;
            this.successRate = successRate;
            this.maintainRate = maintainRate;
        }
    }
}