using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    public interface IValueList<T> where T : struct
    {
        
    } 
    
    [Serializable]
    public sealed class NewCardState : IDeepClonable<NewCardState>
    {
        [SerializeField, VisibleOnly] private CardSpec originalSpec;
        
        [field: Header("Value List")]
        [field: SerializeField] public ValueList<CardColor> ColorList { get; set; }
        [field: SerializeField] public ValueList<int> NumberList { get; set; }
        [field: SerializeField] public ValueList<Direction> DirectionList { get; set; }
        [field: SerializeField] public DirectionFlag DirectionFlag { get; set; }

        
        public bool IsAttack => originalSpec.IsAttack;
        public bool IsMove => originalSpec.IsMove;
        public bool IsStar => NumberList?.AllCandidateValue.Count == 9;

        public UpgradePath UpgradePath => originalSpec.UpgradeNode?.Path ?? UpgradePath.None;
        public bool ValueSelected
        {
            get
            {
                switch (originalSpec.Type)
                {
                    case CardType.Attack:
                        return ColorList.IsFixed && NumberList.IsFixed;
                    
                    case CardType.Move:
                        return DirectionList.IsFixed;
                    
                    default: throw new ArgumentOutOfRangeException(nameof(originalSpec.Type));
                }
            }
        }

        private int Id => originalSpec.ID;
        private CardType Type => originalSpec.Type;
        
        
        public NewCardState DeepClone()
        {
            var clone = (NewCardState)MemberwiseClone();

            clone.ColorList = ColorList?.DeepClone();
            clone.NumberList = NumberList?.DeepClone();
            clone.DirectionList = DirectionList?.DeepClone();
            
            clone.originalSpec = originalSpec;
            return clone;
        }
        
        
        [Serializable]
        public sealed class ValueList<T> : IValueList<T>, IDeepClonable<ValueList<T>> where T : struct
        {
            [field: SerializeField, VisibleOnly] public Optional<T> DefaultValue { get; private set; }
            [field: SerializeField, VisibleOnly] public List<Optional<T>> Alternatives { get; private set; }
            [field: Space, SerializeField, VisibleOnly] public Optional<T> SelectedValue { get; private set; }

            private List<T?> _allCandidateValue;
            
            public bool IsResolved { get; }
            public bool HasAlternatives => Alternatives != null && Alternatives.Count > 0;

            public List<T?> AllCandidateValue => _allCandidateValue;

            public bool IsFixed
            {
                get
                {
                    // 기본값이 존재하고 대안이 없는 경우
                    if (DefaultValue.HasValue && !HasAlternatives) return true;

                    // 선택된 값이 존재하는 경우
                    if (SelectedValue.HasValue) return true;

                    return false;
                }
            }

            public T FixedValue
            {
                get
                {
                    if (!IsFixed)
                        throw new Exception("카드의 값이 고정되지 않았지만 고정된 값에 접근했습니다. IsFixed 체크 후 접근해주세요.");
                    
                    // 기본값이 존재하고 대안이 없는 경우
                    if (DefaultValue.HasValue && !HasAlternatives)
                        return DefaultValue.Value;

                    // 선택된 값이 존재하는 경우
                    if (SelectedValue.HasValue)
                        return SelectedValue.Value;

                    throw new Exception("카드의 값이 고정되지 않았지만 고정된 값에 접근했습니다. IsFixed를 체크 후 접근해주세요.");
                }
            }
            

            public ValueList(T? defaultValue, IReadOnlyList<T> alternatives)
            {
                DefaultValue = new Optional<T>(defaultValue);

                bool hasAlternatives = alternatives != null && alternatives.Count > 0; 
                if (hasAlternatives)
                {
                    Alternatives = alternatives.Select(a => new Optional<T>(a)).ToList();
                }

                _allCandidateValue = new List<T?>();
                if (defaultValue.HasValue)
                {
                    _allCandidateValue.Add(defaultValue.Value);
                }
                if (hasAlternatives)
                {
                    _allCandidateValue.AddRange(alternatives.Select(a => (T?)a));   
                }

                IsResolved = _allCandidateValue.All(v => v.HasValue);
            }
            
            public ValueList(T? defaultValue, IReadOnlyList<T?> alternatives = null)
            {
                DefaultValue = new Optional<T>(defaultValue);

                bool hasAlternatives = alternatives != null && alternatives.Count > 0; 
                if (hasAlternatives)
                {
                    Alternatives = alternatives.Select(a => new Optional<T>(a)).ToList();
                }

                _allCandidateValue = new List<T?>();
                if (defaultValue.HasValue)
                {
                    _allCandidateValue.Add(defaultValue.Value);
                }
                if (hasAlternatives)
                {
                    _allCandidateValue.AddRange(alternatives.Select(a => (T?)a));   
                }

                IsResolved = _allCandidateValue.All(v => v.HasValue);
            }
            
            public ValueList<T> DeepClone()
            {
                var clone = (ValueList<T>)MemberwiseClone();

                if (Alternatives != null)
                {
                    clone.Alternatives = new List<Optional<T>>(Alternatives);
                }

                if (_allCandidateValue != null)
                {
                    clone._allCandidateValue = new List<T?>(_allCandidateValue);
                }

                return clone;
            }

            
            public void Fix(T value)
            {
                if (Alternatives == null || Alternatives.Count == 0)
                {
                    LogEx.LogError("선택 가능한 값이 존재하지 않지만 값을 선택하려 했습니다.");
                    return;
                }

                bool success = false;
                foreach (var alternative in Alternatives)
                {
                    if (alternative.HasValue && alternative.Value.Equals(value))
                    {
                        SelectedValue = new Optional<T>(value);
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    LogEx.LogError("값 선택 실패! 가능하지 않은 값을 선택하려 했습니다.");
                }
            }
        }

        public void ResolveValues()
        {
            if (ColorList != null &&!ColorList.IsResolved)
            {
                var resolvedAlternativeColors = SelectableSlotsResolver
                    .ResolveAlternativeColors(ColorList.DefaultValue, ColorList.Alternatives);
                
                ColorList = new ValueList<CardColor>(ColorList.DefaultValue.Value, resolvedAlternativeColors);
            }

            if (NumberList != null && !NumberList.IsResolved)
            {
                var resolvedAlternativeNumbers = SelectableSlotsResolver
                    .ResolveAlternativeNumbers(NumberList.DefaultValue, NumberList.Alternatives);
                
                NumberList = new ValueList<int>(NumberList.DefaultValue.Value, resolvedAlternativeNumbers);
            }
            
            // Direction은 빌드될 때 이미 Resolve됨.
        }
    }
}