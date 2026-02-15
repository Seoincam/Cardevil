using Cardevil.Attributes;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Common.Core
{
    /// <summary>
    /// 카드의 플레이용 상태 데이터.
    /// </summary>
    public interface ICardState
    {
        CardState.SelectableValues<CardColor> Colors { get; }
        CardState.SelectableValues<int> Numbers { get; }
        CardState.SelectableValues<Direction> Directions { get; }
        DirectionFlag DirectionFlag { get; }
        
        bool IsAttack { get; }
        bool IsMove { get; }
        
        CardState.ValueSelectableType SelectableType { get; }
        bool ValueSelected { get; }
        
        uint Id { get; }
        CardType Type { get; }
    }
    
    /// <summary>
    /// 카드의 플레이용 상태 데이터 클래스.
    /// </summary>
    [Serializable]
    public sealed class CardState : ICardState
    {
        // 해당 상태를 생성한 원본 CardSpec.
        [SerializeField, VisibleOnly] private CardSpec spec;

        // 카드에서 선택 가능한 값의 집함.
        // CardType에 따라 필요한 것만 생성함.
        [field: Space]
        [field: SerializeField] public SelectableValues<CardColor> Colors { get; set; }
        [field: SerializeField] public SelectableValues<int> Numbers { get; set; }
        [field: SerializeField] public SelectableValues<Direction> Directions { get; set; }
        [field: SerializeField] public DirectionFlag DirectionFlag { get; set; }

        public bool IsAttack => spec.IsAttack;
        public bool IsMove => spec.IsMove;

        public ValueSelectableType SelectableType
        {
            get
            {
                if (Colors is { HasAlternatives: true }) return ValueSelectableType.Color;
                if (Numbers is { HasAlternatives: true }) return ValueSelectableType.Number;
                if (Directions is { HasAlternatives: true }) return ValueSelectableType.Direction;
                
                return ValueSelectableType.None;
            }
        }
        
        public bool ValueSelected
        {
            get
            {
                switch (spec.Type)
                {
                    case CardType.Attack: 
                        return Colors.HasSelected && Numbers.HasSelected;
                    
                    case CardType.Move: 
                        return Directions.HasSelected;
                    
                    default: throw new ArgumentOutOfRangeException(nameof(CardState));
                }
            }
        }
        
        public uint Id => spec.ID;
        public CardType Type => spec.Type;

        public CardState(CardSpec spec)
        {
            this.spec = spec;
        }

        /// <summary>
        /// 기본값과 선택 가능 값들을 관리하는 컨테이너.
        /// </summary>
        [Serializable]
        public sealed class SelectableValues<T> where T : struct
        {
            [field: SerializeField] public Optional<T> DefaultValue { get; private set; }
            [SerializeField] private List<T> alternatives = new();
            [SerializeField] private Optional<T> selected;
            
            public bool Initialized { get; }
            public IReadOnlyList<T> Alternatives => alternatives;

            /// <summary>
            /// 현재 유효한 값.
            /// 선택 가능하지만 아직 미선택했다면 null을 반환함.
            /// </summary>
            public T? Current
            {
                get
                {
                    if (HasAlternatives && selected.hasValue) return selected;
                    if (HasAlternatives && !selected.hasValue) return null;
                    
                    if (DefaultValue.hasValue) return DefaultValue;
                    return null;
                }
            }

            /// <summary>
            /// 선택 가능한 모든 값의 개수. 선택이 불가능한 카드일 경우 1임.
            /// </summary>
            public int AllOptionsCount => (alternatives?.Count ?? 0) + 1;

            /// <summary>
            /// 기본값을 포함해, 선택 가능한 모든 값.
            /// </summary>
            public IEnumerable<T> AllOptions
            {
                get
                {
                    if (DefaultValue.hasValue)
                    {
                        yield return DefaultValue.value;   
                    }
                    
                    foreach (var alternative in alternatives)
                    {
                        yield return alternative;
                    }
                }
            }
            
            public bool HasAlternatives => alternatives is { Count: > 0 };
            public bool HasSelected => Current != null;
            
            public SelectableValues(T? defaultValue)
            {
                Initialized = true;
                DefaultValue = new Optional<T>(defaultValue);
            }
            
            public void AddAlternative(T value) => alternatives.Add(value);
            
            public void Select(T value)
            {
                if ((DefaultValue.hasValue && !value.Equals(DefaultValue.value)) 
                    && !alternatives.Contains(value))
                {
                    Debug.LogError($"기본값과 선택 가능한 값 외에 값이 선택됐습니다: {value}. 기본값: {DefaultValue}, 선택 가능한 값: {alternatives}");
                    return;
                }
                
                selected = new Optional<T>(value);   
            }

            public void Unselect()
            {
                selected = new Optional<T>(null);
            }
        }
        
        /// <summary>
        /// 카드의 선택 가능 종류.
        /// </summary>
        public enum ValueSelectableType
        {
            None,
            Color,
            Number,
            Direction
        }
    }
}