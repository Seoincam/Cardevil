using Cardevil.Attributes;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Core;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Data.InStage
{
    [Serializable]
    public sealed class CardData : IClearable
    {
        [Header("Common")]
        [SerializeField, VisibleOnly] private int id;
        [SerializeField, VisibleOnly] private CardKind kind;
        
        [Header("Attack Card")]
        [SerializeField, VisibleOnly] private CardColor color;
        [SerializeField, VisibleOnly] private float damageMultiplier;
        [SerializeField, VisibleOnly] private SelectState<int> numberSelectState;

        [Header("Move Card")]
        [SerializeField, VisibleOnly] private int length;
        [SerializeField, VisibleOnly] private SelectState<Direction> directionSelectState;
        [SerializeField, VisibleOnly] private DirectionFlag directionFlag;

        public bool IsAttack => kind == CardKind.Attack;
        public bool IsMove => kind == CardKind.Move;

        /// <summary>
        /// 값이 확정된 시점에 호출해야함.
        /// 공격카드의 최종 선택 숫자.
        /// </summary>
        public int FinalNumber => (int)numberSelectState.FinalValue;

        /// <summary>
        /// 값이 확정된 시점에 호출해야함.
        /// 이동카드의 최종 선택 방향.
        /// </summary>
        public Direction FinalDirection => (Direction)directionSelectState.FinalValue; 
        
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
        public CardColor Color => color;
        public float DamageMultiplier => damageMultiplier;
        public SelectState<int> NumberSelectState => numberSelectState;

        // Move Card
        public int Length => length;
        public SelectState<Direction> DirectionSelectState => directionSelectState;
        public DirectionFlag DirectionFlag => directionFlag;
        
        // Etc
        public bool CanOpenSelection =>
            kind switch
            {
                CardKind.Attack => numberSelectState.Selectables.Count > 1,
                CardKind.Move => directionSelectState.Selectables.Count > 1,
                _ => throw new ArgumentOutOfRangeException()
            };

        #region Builder
        
        public static Builder CreateBuilder(int id, CardKind kind) => new(id, kind);
        
        public sealed class Builder
        {
            private readonly int _id;
            private readonly CardKind _kind;
            
            private CardColor _color = CardColor.None;
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
                
                SelectState<int> numberSelectState = null;
                SelectState<Direction> directionSelectState = null;

                if (_kind == CardKind.Attack)
                    numberSelectState = new(_numberSelectables);
                else if (_kind == CardKind.Move)
                {
                    directionSelectState = new(_directionSelectables);
                    
                    // Direction Flag 확정
                    foreach (var dir in directionSelectState.Selectables)
                        _directionFlag |= dir.value.ToDirectionFlag();
                }

                return new CardData(_id, _kind, _currentEnhancement, _color, _damageMultiplier, numberSelectState, _length, directionSelectState, _directionFlag);
            }
        }
        
        private CardData(
            int id, CardKind kind, EnhancementData currentEnhancement, 
            CardColor color, float damageMultiplier, SelectState<int> numberSelectState, 
            int length, SelectState<Direction> directionSelectState, DirectionFlag directionFlag)
        {
            this.id = id;
            this.kind = kind;
            CurrentEnhancement = currentEnhancement;

            this.color = color;
            this.damageMultiplier = damageMultiplier;
            this.numberSelectState = numberSelectState;
            
            this.length = length;
            this.directionSelectState = directionSelectState;
            this.directionFlag = directionFlag;
        }

        #endregion
    }
}