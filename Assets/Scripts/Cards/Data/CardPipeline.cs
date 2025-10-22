using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.Modifiers;
using System.Collections.Generic;

namespace Cardevil.Cards.Data
{
    public class CardPipeline
    {
        public int Id { get; }
        public CardKind Kind { get; }
        
        private readonly List<IModifier> _mods = new();
        
        private readonly List<EnhancementData> _possibleEnhancements = new();
        private EnhancementData? _currentEnhancement;
        
        public IReadOnlyList<IModifier> Modifiers => _mods;

        public CardPipeline(CardKind kind, int id)
        {
            Kind = kind;
            Id = id;
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
    }
}