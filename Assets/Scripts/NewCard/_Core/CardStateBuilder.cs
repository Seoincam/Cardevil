using Cardevil.NewCard.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    /// <summary>
    /// <see cref="CardSpec"/>을 기반으로 <see cref="CardState"/>를 생성하는 빌더.
    /// </summary>
    public sealed class CardStateBuilder
    {
        // 해당 속성의 기본값들.
        private CardColor? _defaultColor;
        private int? _defaultNumber;
        private Direction? _defaultDirection;

        // 기본값 외 선택 가능한 슬롯 목록들.
        private readonly List<SelectableSlot<CardColor>> _colorSelectableSlots = new();
        private readonly List<SelectableSlot<int>> _numberSelectableSlots = new();
        private readonly List<SelectableSlot<Direction>> _directionSelectableSlots = new();
        
        public void SetDefaultColor(CardColor color) => _defaultColor = color;
        public void SetDefaultNumber(int number) => _defaultNumber = number;
        public void SetDefaultDirection(Direction direction) => _defaultDirection = direction;
        
        public void AddColorSelectableSlot(SelectableSlot<CardColor> colorSelectable) => _colorSelectableSlots.Add(colorSelectable);
        public void AddNumberSelectableSlot(SelectableSlot<int> numberSelectable) => _numberSelectableSlots.Add(numberSelectable);
        public void AddDirectionSelectableSlot(SelectableSlot<Direction> directionSelectable) => _directionSelectableSlots.Add(directionSelectable);
        
        [Serializable]
        public struct SelectableSlot<T> where T : struct
        {
            [field: SerializeField] public bool IsFixed {get; private set;}
            [field: SerializeField] public T FixedValue {get; private set;}

            public static SelectableSlot<T> Fixed(T value) => new() { IsFixed = true, FixedValue = value };
            public static SelectableSlot<T> Random() => new() { IsFixed = false };
        }

        /// <summary>
        /// Spec Elements를 적용해 CardState 생성.
        /// CardType에 따라 필요한 값만 생성함.
        /// </summary>
        public CardState Build(CardSpec spec)
        {
            Clear();
            
            foreach (var element in spec.Elements)
                element.Apply(this);
            
            var state = new CardState(spec);

            if (spec.Type == CardType.Attack)
            {
                var resolvedColors = SelectableSlotsResolver.ResolveColors(_defaultColor, _colorSelectableSlots);
                state.Colors = BuildSelectable(_defaultColor, resolvedColors);
                
                var resolvedNumbers = SelectableSlotsResolver.ResolveNumbers(_defaultNumber, _numberSelectableSlots);
                state.Numbers = BuildSelectable(_defaultNumber, resolvedNumbers);
            }
            else if (spec.Type == CardType.Move)
            {
                var resolvedDirection = SelectableSlotsResolver.ResolveDirections(_directionSelectableSlots);
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

        // 이전 빌드 상태 초기화. 모든 변수를 초기화함.
        private void Clear()
        {
            _defaultColor = null;
            _defaultNumber = null;
            _defaultDirection = null;
            _colorSelectableSlots.Clear();
            _numberSelectableSlots.Clear();
            _directionSelectableSlots.Clear();
        }
    }
}