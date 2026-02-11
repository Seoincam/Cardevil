using Cardevil.Attributes;
using Cardevil.Utils;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    /// <summary>
    /// 카드의 플레이용 상태 데이터.
    /// </summary>
    public interface ICardState
    {
        CardState.SelectableValues<CardColor> Colors { get; }
        CardState.SelectableValues<int> Numbers { get; }
        CardState.SelectableValues<Direction> Directions { get; }
        
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

            private T? _selected;
            
            public IReadOnlyList<T> Alternatives => alternatives;

            /// <summary>
            /// 현재 유효한 값.
            /// 선택 가능하지만 아직 미선택했다면 null을 반환함.
            /// </summary>
            public T? Current => HasAlternatives 
                ? _selected 
                : (DefaultValue.hasValue ? DefaultValue.value : null);

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
            
            public bool HasAlternatives => alternatives.Count > 0;
            public bool HaSelected => Current != null;
            
            public SelectableValues(T? defaultValue)
            {
                DefaultValue = new Optional<T>(defaultValue);
            }
            
            public void AddAlternative(T value) => alternatives.Add(value);
            
            public void Select(T value)
            {
                if (!value.Equals(DefaultValue) && !alternatives.Contains(value))
                {
                    Debug.LogError($@"기본값과 선택 가능한 값 외에 값이 선택됐습니다: {value}\n기본값: {DefaultValue}, 선택 가능한 값: {alternatives}");
                    return;
                }
                
                _selected = value;   
            }

            public void Unselect()
            {
                _selected = null;
            }
        }
    }
}