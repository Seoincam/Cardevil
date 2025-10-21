using Cardevil.Cards.Data.InStage;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.Modifiers
{
    public enum ModifierType
    {
        AttackColor,
        AttackDamage,
        AttackNumSelectable,
        AttackNumSelectableConfirm,
        
        MoveDirSelectable
    }

    public class BuildCardContext
    {
        
    }
    
    public interface IModifier
    {
        ModifierType Type { get; }

        void Apply(BuildCardContext ctx);
    }

    public class CardPipeline
    {
        public int id;
        public CardKind kind;
        
        private readonly List<IModifier> _mods = new();
        
        private readonly List<EnhancementData> _possibleEnhancements = new();
        private EnhancementData? _currentEnhancement;

        public CardPipeline(CardKind kind, int id)
        {
            this.kind = kind;
            this.id = id;
        }

        public void AddModifier(IModifier mod)
        {
            _mods.Add(mod);
        }

        public void SetPossibleEnhancements(params EnhancementData[] possibles)
        {
            _possibleEnhancements.Clear();

            foreach (var enhancement in possibles)
                _possibleEnhancements.Add(enhancement);
        }

        public BuiltCardData Build()
        {
            return null;
        }
    }
    
    public enum CardKind { Attack, Move }

    public class BuiltCardData
    {
        public int id;
        public CardKind kind;
        
        public CardColor color;
        public float damageMultiplier;
        public SelectState<int> numberSelectState;

        public int length;
        public SelectState<Direction> directionSelectState;
    }

    public readonly struct EnhancementData
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