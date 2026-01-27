using Cardevil.Attributes;
using Cardevil.Cards.Enhancements;
using Cardevil.Core;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    [Serializable]
    public sealed class CardData : IClearable
    {
        [Header("Common")]
        [SerializeField, VisibleOnly] private int id;
        [SerializeField, VisibleOnly] private CardKind kind;
        
        [Header("Attack Card")]
        [SerializeField, VisibleOnly] private float damageMultiplier;
        [SerializeField, VisibleOnly] private SelectState<int> numberSelectState;
        [SerializeField, VisibleOnly] private SelectState<CardColor> colorSelectState;

        [Header("Move Card")]
        [SerializeField, VisibleOnly] private int length;
        [SerializeField, VisibleOnly] private SelectState<Direction> directionSelectState;
        [SerializeField, VisibleOnly] private DirectionFlag directionFlag;
        
        /// <summary>
        /// 스테이지 입장 전 상태로 초기화합니다.
        /// </summary>
        public void Clear()
        {
            switch (kind)
            {
                case CardKind.Attack: numberSelectState.Clear(); break;
                case CardKind.Move: directionSelectState.Clear(); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        
        // Common
        public int Id => id;
        public CardKind Kind => kind;
        public EnhancementData CurrentEnhancement { get; }
        
        // Attack Card
        public float DamageMultiplier => damageMultiplier;
        public SelectState<int> NumberSelectState => numberSelectState;
        public SelectState<CardColor> ColorSelectState => colorSelectState;

        // Move Card
        public int Length => length;
        public SelectState<Direction> DirectionSelectState => directionSelectState;
        public DirectionFlag DirectionFlag => directionFlag;
        
        // Etc
        public bool CanOpenSelection => 
            IsAttack ? numberSelectState.Selectables.Count > 1 : directionSelectState.Selectables.Count > 1; 
        
        public bool IsAttack => kind == CardKind.Attack;
        public bool IsMove => kind == CardKind.Move;

        /// <summary>
        /// 오망성 카드인가 여부.
        /// </summary>
        public bool IsStar => IsAttack && numberSelectState.Selectables.Count == 9;

        /// <summary>
        /// 선택 가능한 값들의 수.
        /// </summary>
        public int SelectableCount =>
            IsAttack ? numberSelectState.Selectables.Count : directionSelectState.Selectables.Count;

        /// <summary>
        /// 값 선택을 완료했는가 여부.
        /// 선택 가능한 값이 하나인 경우에도 <c>true</c>를 반환함.
        /// </summary>
        public bool CompleteSelectingValue =>
            IsAttack 
                ? numberSelectState.FinalValue.HasValue && colorSelectState.FinalValue.HasValue 
                : directionSelectState.FinalValue.HasValue;

        /// <summary>
        /// 공격 카드의 최종 선택 숫자.
        /// </summary>
        public int FinalNumber => CompleteSelectingValue
                ? (int)numberSelectState.FinalValue
                : throw new ArgumentNullException(nameof(numberSelectState.FinalValue));
        
        public CardColor FinalColor => CompleteSelectingValue 
            ? (CardColor)colorSelectState.FinalValue 
            : throw new ArgumentNullException(nameof(colorSelectState.FinalValue));

        /// <summary>
        /// 이동 카드의 최종 선택 방향.
        /// </summary>
        public Direction FinalDirection => CompleteSelectingValue 
            ? (Direction)directionSelectState.FinalValue
            : throw new ArgumentNullException(nameof(directionSelectState.FinalValue));

        #region Builder
        
        public static Builder CreateBuilder(int id, CardKind kind) => new(id, kind);
        
        public sealed class Builder
        {
            private readonly int _id;
            private readonly CardKind _kind;

            private readonly List<CardColor?> _colorSelectables = new();
            private float _damageMultiplier = 1f;
            private readonly List<int?> _numberSelectables = new();

            private int _length = 1;
            private readonly List<Direction?> _directionSelectables = new();
            private DirectionFlag _directionFlag = DirectionFlag.None;
            
            private EnhancementData _currentEnhancement;
            
            public IReadOnlyList<int?> NumberSelectables => _numberSelectables;
            public IReadOnlyList<Direction?> DirectionSelectables => _directionSelectables;

            public Builder(int id, CardKind kind)
            {
                _id = id;
                _kind = kind;
            }

            #region Setter

            public void AddColorSelectable(CardColor? color)
            {
                if (!color.HasValue)
                {
                    _colorSelectables.Add(null);
                    return;
                }

                // color.HasValue일 경우, 기존의 color를 number로 대체
                for (int i = 0; i < _numberSelectables.Count; i++)
                {
                    if (_colorSelectables[i].HasValue) continue;
                    _colorSelectables[i] = color;
                    break;
                }
            }
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
                    if (!_directionSelectables[i].HasValue)
                    {
                        _directionSelectables[i] = direction;
                        return;
                    }
                }
                
                // 기존 null이 없을 경우 새로 추가
                _directionSelectables.Add(direction);
            }
            
            public void SetCurrentEnhancement(EnhancementData currentEnhancement) => _currentEnhancement = currentEnhancement;

            #endregion
            
            public CardData Build()
            {
                if (_kind == CardKind.Attack && _numberSelectables.Count == 9)
                {
                    _numberSelectables.Clear();
                    _numberSelectables.AddRange(new int?[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
                }
                
                if (_kind == CardKind.Move && _directionSelectables.Count == 4)
                {
                    _directionSelectables.Clear();
                    _directionSelectables.AddRange(new Direction?[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right });
                }

                SelectState<CardColor> colorSelectState = null;
                SelectState<int> numberSelectState = null;
                SelectState<Direction> directionSelectState = null;

                if (_kind == CardKind.Attack)
                {
                    numberSelectState = new SelectState<int>(_numberSelectables);
                    colorSelectState = new SelectState<CardColor>(_colorSelectables);
                }
                else if (_kind == CardKind.Move)
                {
                    directionSelectState = new SelectState<Direction>(_directionSelectables);
                    
                    // Direction Flag 확정
                    foreach (var dir in directionSelectState.Selectables)
                        _directionFlag |= dir.value.ToDirectionFlag();
                }

                return new CardData(_id, _kind, _currentEnhancement, colorSelectState, _damageMultiplier, numberSelectState, _length, directionSelectState, _directionFlag);
            }
        }
        
        private CardData(
            int id, CardKind kind, EnhancementData currentEnhancement, 
            SelectState<CardColor> colorSelectState, float damageMultiplier, SelectState<int> numberSelectState, 
            int length, SelectState<Direction> directionSelectState, DirectionFlag directionFlag)
        {
            this.id = id;
            this.kind = kind;
            CurrentEnhancement = currentEnhancement;

            this.colorSelectState = colorSelectState;
            this.damageMultiplier = damageMultiplier;
            this.numberSelectState = numberSelectState;
            
            this.length = length;
            this.directionSelectState = directionSelectState;
            this.directionFlag = directionFlag;
        }

        #endregion
    }
}