using Cardevil.Cards.Data.Enhancement;
using Cardevil.Utils.Directions;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public sealed class CardData
    {
        // Common
        public int Id { get; }
        public CardKind Kind { get; }
        public EnhancementData? CurrentEnhancement { get; }
        
        // Attack Card
        public CardColor Color { get; }
        public float DamageMultiplier { get; }
        public SelectState<int> NumberSelectState { get; }

        // Move Card
        public int Length { get; }
        public SelectState<Direction> DirectionSelectState { get; }
        
        #region Builder
        
        public static Builder CreateBuilder(int id, CardKind kind) => new(id, kind);
        
        public sealed class Builder
        {
            private readonly int _id;
            private readonly CardKind _kind;
            
            private CardColor _color;
            private float _damageMultiplier = 1f;
            private readonly List<int?> _numberSelectables = new();

            private int _length = 1;
            private readonly List<Direction?> _directionSelectables = new();
            
            private EnhancementData? _currentEnhancement;
            
            public IReadOnlyList<int?> NumberSelectables => _numberSelectables;
            public IReadOnlyList<Direction?> DirectionSelectables => _directionSelectables;

            public Builder(int id, CardKind kind)
            {
                _id = id;
                _kind = kind;
            }

            #region Setter

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

            #endregion
            
            public CardData Build()
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

                return new CardData(_id, _kind, _currentEnhancement, _color, _damageMultiplier, numberSelectState, _length, directionSelectState);
            }
        }
        
        private CardData(
            int id, CardKind kind, EnhancementData? currentEnhancement, 
            CardColor color, float damageMultiplier, SelectState<int> numberSelectState, 
            int length, SelectState<Direction> directionSelectState)
        {
            Id = id;
            Kind = kind;
            CurrentEnhancement = currentEnhancement;

            Color = color;
            DamageMultiplier = damageMultiplier;
            NumberSelectState = numberSelectState;
            
            Length = length;
            DirectionSelectState = directionSelectState;
        }

        #endregion
    }
}