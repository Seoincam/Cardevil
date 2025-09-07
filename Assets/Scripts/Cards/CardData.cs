using Cardevil.Utils.Directions;
using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    [Serializable]
    public sealed class CardData : ICopyable<CardData>, ILockable
    {
        [HideInInspector] public int id;

        [Header("Values")]
        public ValueType valueType;
        [SerializeField] NumberData _defaultNumber;
        [SerializeField] MoveData _defaultMove;

        [Header("Selections")]
        public SelectType selectType;
        [SerializeField] NumberData _selectedNumber;
        [SerializeField] MoveData _selectedMove;

        [Space, SerializeField] int _numberOptionCount = 0;
        private HashSet<int> _numberOptions;

        [Header("States")]
        public bool isLocked = false;



        public NumberData DefaultNumber => _defaultNumber;

        public MoveData DefaultMove => _defaultMove;

        /// <summary>
        /// 카드의 최종 Number 값
        /// </summary>
        public NumberData Number
        {
            get => _selectedNumber.isSet ? _selectedNumber : _defaultNumber;
        }

        /// <summary>
        /// 카드의 최종 Move 값
        /// </summary>
        public MoveData Move
        {
            get => _selectedMove.isSet ? _selectedMove : _defaultMove;
        }

        /// <summary>
        /// 기본값 외에 선택 가능한 Number 옵션의 개수
        /// </summary>
        public int NumberOptionCount => _numberOptionCount;

        /// <summary>
        /// 기본값 외에 선택 가능한 Number 옵션
        /// </summary>
        public HashSet<int> NumberOptions => _numberOptions;

        /// <summary>
        /// 사용 가능 여부를 반환
        /// </summary>
        public bool CanUse
        {
            get => IsValueSelected && !isLocked;
        }

        /// <summary>
        /// 값 선택 가능 여부를 반환
        /// </summary>
        public bool CanOpenSelection
        {
            get => !isLocked && selectType > 0;
        }

        // 값 선택 완료 여부를 반환
        public bool IsValueSelected
        {
            get
            {
                if (selectType == SelectType.None)
                    return true;
                return (valueType == ValueType.Number && _selectedNumber.isSet) ||
                    (valueType == ValueType.Move && _selectedMove.isSet);
            }
        }



        /// <summary>
        /// 스테이지에서 카드를 뽑을 때 실행
        /// </summary>
        public void OnDraw()
        {
            // TODO: 카드를 다시 뽑을 때 선택 가능 값도 초기화되나 확인
            if (valueType == ValueType.Number && selectType == SelectType.Multiple)
            {
                if (_numberOptionCount == 0)
                    return;

                _numberOptions = new(_numberOptionCount);
                while (_numberOptions.Count < _numberOptionCount)
                {
                    int random = Random.Range(2, 11);
                    if (random != _defaultNumber.number)
                        _numberOptions.Add(random);
                }
            }
        }

        /// <summary>
        /// 선택된 카드의 Number 값을 할당
        /// </summary>
        public void SelectValue(int selectNumber)
        {
            var number = new NumberData(DefaultNumber.color, selectNumber);
            _selectedNumber = number;
        }

        /// <summary>
        /// 선택된 카드의 Direction 값을 할당
        /// </summary>
        public void SelectValue(Direction selectDirection)
        {
            var move = new MoveData(selectDirection, 1);
            _selectedMove = move;
        }

        /// <summary>
        /// 카드를 사용 불가하게 잠금. 뽑기는 가능.
        /// </summary>
        public void Lock()
        {
            isLocked = true;
        }

        [Header("Reinforce")]
        [Tooltip("카드가 강화 가능한가?")]
        public bool reinforceEnabled = false;


        // 강화
        // TODO: 강화 로직 구현하기
        /*
        public enum NumberReinforceMode { None, Damage, SelectCount }
        public NumberReinforceState NumberState = new() { Mode = NumberReinforceMode.None, level = 0 };
        public MoveReinforceState DirectionState = new() { level = 0 };

        [Serializable]
        public struct NumberReinforceState
        {
            public NumberReinforceMode Mode;
            [Min(0), Tooltip("강화 단계 (기본 0)")]
            public int level;
        }

        [Serializable]
        public struct MoveReinforceState
        {
            [Min(0), Tooltip("강화 단계 (기본 0)")]
            public int level;
        }
        */



        /// <summary>
        /// 카드가 사용할 값의 종류를 나타냄.
        /// </summary>
        public enum ValueType
        {
            Number,
            Move
        }

        /// <summary>
        /// 카드의 선택 옵션 종류를 나타냄.
        /// </summary>
        public enum SelectType
        {
            // 옵션 추가될 수 있으므로 값을 이렇게 잡음
            None = -1,
            Multiple = 1,
            All = 10
        }

        public CardData() {}

        public CardData(int id, NumberData defaultNubmer, MoveData defaultMove, ValueType valueType, SelectType selectType = SelectType.None)
        {
            this.id = id;
            _defaultNumber = defaultNubmer;
            _defaultMove = defaultMove;
            _selectedNumber = new();
            _selectedMove = new();
            this.valueType = valueType;
            this.selectType = selectType;
        }
        
        public CardData Copy()
        {
            return new CardData()
            {
                id = id,
                valueType = valueType,
                selectType = selectType,
                _defaultNumber = _defaultNumber,
                _defaultMove = _defaultMove,
                _selectedNumber = new(),
                _selectedMove = new(),
                isLocked = false,
                _numberOptionCount = _numberOptionCount,
                reinforceEnabled = reinforceEnabled
            };
        }
    }


    [Serializable]
    public class NumberData : ICopyable<NumberData>
    {
        public bool isSet;
        public enum CardColor { None = -1, Red = 0 , Green = 1, Blue = 2, Black = 3 }
        public CardColor color;
        public int number;

        public NumberData()
        {
            isSet = false;
            color = CardColor.None;
            number = 0;
        }

        public NumberData(CardColor color, int number)
        {
            isSet = true;
            this.color = color;
            this.number = number;
        }

        public NumberData Copy()
        {
            return new NumberData(color, number);
        }

    }

    [Serializable]
    public class MoveData : ICopyable<MoveData>
    {
        public bool isSet;
        public Direction direction;
        public int length;

        public MoveData()
        {
            isSet = false;
            direction = Direction.None;
            length = 0;
        }

        public MoveData(Direction direction, int length)
        {
            isSet = true;
            this.direction = direction;
            this.length = length;
        }

        public MoveData Copy()
        {
            return new MoveData(direction, length);
        }

    }

    public interface ILockable
    {
        void Lock();
    }

    /// <summary>
    /// DeepCopy를 하고 반환하는 클래스가 구현함.
    /// </summary>
    public interface ICopyable<T>
    {
        T Copy();
    }
}
