using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    public interface ICardState
    {
        CardState.SelectableValues<CardColor> Colors { get; }
        CardState.SelectableValues<int> Numbers { get; }
        CardState.SelectableValues<Direction> Directions { get; }
        
        uint Id { get; }
        CardType Type { get; }
    }
    
    public sealed class CardState : ICardState
    {
        private readonly CardSpec _spec;

        public SelectableValues<CardColor> Colors { get; set; }
        public SelectableValues<int> Numbers { get; set; }
        public SelectableValues<Direction> Directions { get; set; }

        public uint Id => _spec.ID;
        public CardType Type => _spec.Type;
        
        public CardState(CardSpec spec)
        {
            _spec = spec;
        }

        [Serializable]
        public sealed class SelectableValues<T> where T : struct
        {
            [field: SerializeField] public T DefaultValue { get; private set; }
            [SerializeField] private List<T> alternatives = new();

            private T? _selected;
            
            public IReadOnlyList<T> Alternatives => alternatives;

            /// <summary>
            /// 현재 유효한 값.
            /// 선택 가능하지만 아직 미선택했다면 null을 반환함.
            /// </summary>
            public T? Current => HasAlternatives ? _selected : DefaultValue;

            /// <summary>
            /// 기본값을 포함해, 선택 가능한 모든 값.
            /// </summary>
            public IEnumerable<T> AllOptions
            {
                get
                {
                    yield return DefaultValue;
                    foreach (var alternative in alternatives)
                    {
                        yield return alternative;
                    }
                }
            }
            
            public bool HasAlternatives => alternatives.Count > 0;
            public bool HaSelected => Current != null;
            
            public SelectableValues(T defaultValue)
            {
                DefaultValue = defaultValue;
            }
            
            public void AddAlternative(T value) => alternatives.Add(value);

            public void Select(T value)
            {
                if (value.Equals(DefaultValue) || alternatives.Contains(value))
                {
                    _selected = value;   
                }
            }

            public void Unselect()
            {
                _selected = null;
            }
        }
    }
}