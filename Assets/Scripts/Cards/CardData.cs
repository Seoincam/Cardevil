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
        [Header("Values")]
        public ValueType valueType;
        [SerializeField] NumberData _defaultNumber;
        [SerializeField] MoveData _defaultMove;

        [Header("Selections")]
        public SelectType selectType;
        [SerializeField] NumberData _selectedNumber = null;
        [SerializeField] MoveData _selectedMove = null;

        // 기본값 외에 선택 가능한 Number 옵션의 개수
        [Space, SerializeField] int numberOptionCount = 0;
        private HashSet<int> numberOptions;

        [Header("States")]
        public bool isLocked = false;

        // 카드의 최종 Number 값
        public NumberData Number => _selectedNumber ?? _defaultNumber;
        // 카드의 최종 Move 값
        public MoveData Move => _selectedMove ?? _defaultMove;

        // 사용 가능한가?
        public bool CanUse => IsValueSelected && !isLocked;

        // 값 선택 가능 여부를 반환
        public bool CanOpenSelection
        {
            get => !isLocked && selectType > 0;
        }

        // 값 선택 완료 여부를 반환
        private bool IsValueSelected
        {
            get
            {
                if (selectType == SelectType.None)
                    return true;
                return (valueType == ValueType.Number && _selectedNumber != null) ||
                    (valueType == ValueType.Move && _selectedMove != null);
            }
        }

        // 스테이지에서 카드를 뽑을 때 실행.
        public void OnDraw()
        {
            if (valueType == ValueType.Number && selectType == SelectType.Multiple)
            {
                if (numberOptionCount == 0)
                    return;

                numberOptions = new(numberOptionCount);
                while (numberOptions.Count < numberOptionCount)
                {
                    int random = Random.Range(2, 11);
                    if (random != _defaultNumber.number)
                        numberOptions.Add(random);
                }
            }
        }

        public void Lock()
        {
            isLocked = true;
        }

        public CardData Copy()
        {
            return new CardData()
            {
                valueType = valueType,
                selectType = selectType,
                _defaultNumber = _defaultNumber,
                _defaultMove = _defaultMove,
                isLocked = false,
                numberOptionCount = numberOptionCount,
                reinforceEnabled = reinforceEnabled
            };
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
            // 옵션 추가될 수 있으므로 값을 이렇게 잡음.
            None = -1,
            Multiple = 1,
            All = 10
        }
        
        public CardData() {}
        public CardData(NumberData defaultNubmer, MoveData defaultMove, ValueType valueType, SelectType selectType = SelectType.None)
        {
            _defaultNumber = defaultNubmer;
            _defaultMove = defaultMove;
            this.valueType = valueType;
            this.selectType = selectType;
        }
    }


    [Serializable]
    public class NumberData : ICopyable<NumberData>
    {
        public enum CardColor { None, Red, Blue, Green, Black }
        public CardColor color = CardColor.None;
        public int number = 0;

        public NumberData Copy()
        {
            return new NumberData() { color = color, number = number };
        }

    }

    [Serializable]
    public class MoveData : ICopyable<MoveData>
    {
        public Direction direction = Direction.None;
        public int length = 0;

        public MoveData Copy()
        {
            return new MoveData() { direction = direction, length = length };
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
