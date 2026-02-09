using Cardevil.NewCard.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    public sealed class CardStateBuilder
    {
        private CardColor? _defaultColor;
        private int? _defaultNumber;
        private Direction? _defaultDirection;

        private readonly List<AlternativeSlot<CardColor>> _colorAlternatives = new();
        private readonly List<AlternativeSlot<int>> _numberAlternatives = new();
        private readonly List<AlternativeSlot<Direction>> _directionAlternatives = new();
        
        public void SetDefaultColor(CardColor color) => _defaultColor = color;
        public void SetDefaultNumber(int number) => _defaultNumber = number;
        public void SetDefaultDirection(Direction direction) => _defaultDirection = direction;
        
        public void AddColorAlternative(AlternativeSlot<CardColor> colorAlternative) => _colorAlternatives.Add(colorAlternative);
        public void AddNumberAlternative(AlternativeSlot<int> numberAlternative) => _numberAlternatives.Add(numberAlternative);
        public void AddDirectionAlternative(AlternativeSlot<Direction> directionAlternative) => _directionAlternatives.Add(directionAlternative);
        
        [Serializable]
        public struct AlternativeSlot<T> where T : struct
        {
            [field: SerializeField] public bool IsFixed {get; private set;}
            [field: SerializeField] public T FixedValue {get; private set;}

            public static AlternativeSlot<T> Fixed(T value) => new() { IsFixed = true, FixedValue = value };
            public static AlternativeSlot<T> Random() => new() { IsFixed = false };
        }

        public CardState Build(CardSpec spec)
        {
            Clear();
            
            foreach (var element in spec.Elements)
                element.Apply(this);
            
            var state = new CardState(spec);

            if (spec.Type == CardType.Attack)
            {
                var resolvedColors = AlternativeSlotResolver.ResolveColors(_defaultColor, _colorAlternatives);
                state.Colors = BuildSelectable(_defaultColor, resolvedColors);
                
                var resolvedNumbers = AlternativeSlotResolver.ResolveNumbers(_defaultNumber, _numberAlternatives);
                state.Numbers = BuildSelectable(_defaultNumber, resolvedNumbers);
            }
            else if (spec.Type == CardType.Move)
            {
                var resolvedDirection = AlternativeSlotResolver.ResolveDirections(_directionAlternatives);
                state.Directions = BuildSelectable(_defaultDirection, resolvedDirection);
            }
            
            return state;
        }
        
        private static CardState.SelectableValues<T> BuildSelectable<T>(
            T? defaultValue,
            IReadOnlyList<T> alternatives) where T : struct
        {
            if (!defaultValue.HasValue)
                throw new InvalidOperationException($"기본값을 미설정 : {typeof(T).Name}");
            
            var selectable = new CardState.SelectableValues<T>(defaultValue.Value);
            
            foreach (var alternative in alternatives)
            {
                selectable.AddAlternative(alternative);
            }
            
            return selectable;
        }

        private void Clear()
        {
            _defaultColor = null;
            _defaultNumber = null;
            _defaultDirection = null;
            _colorAlternatives.Clear();
            _numberAlternatives.Clear();
            _directionAlternatives.Clear();
        }
    }
}