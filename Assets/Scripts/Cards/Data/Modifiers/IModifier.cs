using Cardevil.Cards.Data.InStage;
using Cardevil.Utils.Directions;
using System.Collections.Generic;
using System.Linq;

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

    public interface IModifier
    {
        ModifierType Type { get; }

        void Apply(BuiltCardData.Builder b);
    }
    
    public enum CardKind { Attack, Move }

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
    

    public sealed class BuiltCardData
    {
        public int Id { get; }
        public CardKind Kind { get; }
        
        public CardColor Color { get; }
        public float DamageMultiplier { get; }
        public SelectState<int> NumberSelectState { get; }

        public int Length { get; }
        public SelectState<Direction> DirectionSelectState { get; }
        
        public EnhancementData? CurrentEnhancement { get; }

        private BuiltCardData(int id, CardKind kind, CardColor color, float damageMultiplier,
            SelectState<int> numberSelectState, int length, SelectState<Direction> directionSelectState,
            EnhancementData? currentEnhancement)
        {
            Id = id;
            Kind = kind;
            Color = color;
            DamageMultiplier = damageMultiplier;
            NumberSelectState = numberSelectState;
            Length = length;
            DirectionSelectState = directionSelectState;
            CurrentEnhancement = currentEnhancement;
        }

        public static Builder StartBuild(int id, CardKind kind) => new Builder(id, kind);
        
        public sealed class Builder
        {
            private readonly int _id;
            private readonly CardKind _kind;
            
            private CardColor _color;
            private float _damageMultiplier;
            private List<int?> _numberSelectables = new();

            private int _length;
            private List<Direction?> _directionSelectables = new();
            
            private EnhancementData? _currentEnhancement;
            
            public IReadOnlyList<int?> NumberSelectables => _numberSelectables;
            public IReadOnlyList<Direction?> DirectionSelectables => _directionSelectables;

            public Builder(int id, CardKind kind)
            {
                _id = id;
                _kind = kind;
            }
            
            public void SetColor(CardColor color) => _color = color;
            public void AddDamageMultiplier(float multiplier) => _damageMultiplier += multiplier;
            public void AddNumberSelectable(int? number)
            {
                if (!number.HasValue)
                {
                    _numberSelectables.Add(null);
                    return;
                }
                    
                // number.HasValue일 경우, 기존의 null을 number로 대체
                for (int i = 0; i < _numberSelectables.Count; i++)
                {
                    if (_numberSelectables[i].HasValue) continue;
                    _numberSelectables[i] = number;
                    break;
                }
            }
            
            public void SetLength(int length) => _length = length;
            public void AddDirectionSelectable(Direction? direction)
            {
                if (!direction.HasValue)
                {
                    _directionSelectables.Add(null);
                    return;
                }
                
                // direction.HasValue일 경우, 기존의 null을 direction으로 대체
                for (int i = 0; i < _directionSelectables.Count; i++)
                {
                    if (_directionSelectables[i].HasValue) continue;
                    _directionSelectables[i] = direction;
                    return;
                }
                
                _directionSelectables.Add(direction);
            }
            
            public void SetCurrentEnhancement(EnhancementData? currentEnhancement) => _currentEnhancement = currentEnhancement;

            public BuiltCardData Build()
            {
                if (_numberSelectables.Count == 9)
                {
                    _numberSelectables.Clear();
                    _numberSelectables.AddRange(new int?[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
                }
                
                if (_directionSelectables.Count == 4)
                {
                    _directionSelectables.Clear();
                    _directionSelectables.AddRange(new Direction?[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right });
                }
                
                SelectState<int> numberSelectState = null;
                SelectState<Direction> directionSelectState = null;

                if (_kind == CardKind.Attack)
                    numberSelectState = new(_numberSelectables);
                else if (_kind == CardKind.Move)
                    directionSelectState = new(_directionSelectables);

                return new BuiltCardData(_id, _kind, _color, _damageMultiplier, numberSelectState, _length,
                    directionSelectState, _currentEnhancement);
            }
        }
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