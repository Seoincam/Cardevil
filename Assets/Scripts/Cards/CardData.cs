using Cardevil.Core;
using Cardevil.Utils.Directions;
using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    [Serializable]
    public sealed class CardData : IDeepClonable<CardData>, ILockable
    {
        // TODO: 수치 SO 등으로 분리
        private int maxNumberOptionCount = 2;

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

        [Header("Damage")]
        [SerializeField] int _additionalDamage = 0;

        [Header("Reinforcement")]
        public bool reinforceEnabled = false;

        [Header("States")]
        public bool isLocked = false;



        public NumberData DefaultNumber => _defaultNumber;

        public MoveData DefaultMove => _defaultMove;

        /// <summary>
        /// 카드의 추가 데미지 값.
        /// </summary>
        public int AdditionalDamage
        {
            get => _additionalDamage;
            set => _additionalDamage = value;
        }

        /// <summary>
        /// 카드의 최종 Number 값
        /// </summary>
        public NumberData Number
        {
            get => _selectedNumber.IsSet ? _selectedNumber : _defaultNumber;
        }

        /// <summary>
        /// 카드의 최종 Move 값
        /// </summary>
        public MoveData Move
        {
            get => _selectedMove.IsSet ? _selectedMove : _defaultMove;
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

        /// <summary>
        /// 값 선택 완료 여부를 반환
        /// </summary>
        public bool IsValueSelected
        {
            get
            {
                if (selectType == SelectType.None)
                    return true;
                return (valueType == ValueType.Number && _selectedNumber.IsSet) ||
                    (valueType == ValueType.Move && _selectedMove.IsSet);
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
                    if (random != _defaultNumber.NumberValue)
                        _numberOptions.Add(random);
                }
            }
        }

        /// <summary>
        /// 선택된 카드의 Number 값을 할당
        /// </summary>
        public void SelectValue(int selectNumber)
        {
            var number = new NumberData(DefaultNumber.ColorValue, selectNumber);
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

        /// <summary>
        /// 숫자 카드의 데미지를 강화.
        /// </summary>
        public void ReinforceNumberDamage()
        {
            DefaultNumber.ReinforceDamage();
        }

        /// <summary>
        /// 숫자 카드의 선택 옵션을 강화.
        /// </summary>
        public void ReinforceNumberSelect()
        {
            _numberOptionCount++;

            if (_numberOptionCount > maxNumberOptionCount)
            {
                _numberOptionCount = 0;
                selectType = SelectType.All;
            }
        }
        
        /// <summary>
        /// 이동 카드의 선택 옵션을 강화.
        /// </summary>
        public void ReinforceDirectionSelect()
        {
            selectType = selectType switch
            {
                SelectType.None => SelectType.Multiple,
                SelectType.Multiple => SelectType.All,
                _ => SelectType.All
            };
        }



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
        
        public CardData DeepClone()
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
                _additionalDamage = _additionalDamage,
                isLocked = false,
                _numberOptionCount = _numberOptionCount,
                reinforceEnabled = reinforceEnabled
            };
        }
    }


    [Serializable]
    public class NumberData : IDeepClonable<NumberData>
    {
        [SerializeField] bool _isSet;
        [SerializeField] CardColor _colorValue;
        [SerializeField] int _numberValue;
        [SerializeField] int _damageReinforceLevel;


        public bool IsSet => _isSet;

        public CardColor ColorValue => _colorValue;

        public int NumberValue => _numberValue;

        public int DamageReinforceLevel => _damageReinforceLevel;


        public void ReinforceDamage()
        {
            _damageReinforceLevel++;
        }


        public NumberData()
        {
            _isSet = false;
            _colorValue = CardColor.None;
            _numberValue = 0;
        }

        public NumberData(CardColor color, int number)
        {
            _isSet = true;
            _colorValue = color;
            _numberValue = number;
        }


        public NumberData DeepClone()
        {
            return new NumberData(_colorValue, _numberValue);
        }


        public enum CardColor
        {
            None = -1,
            Red = 0,
            Green = 1,
            Blue = 2,
            Black = 3
        }
    }

    [Serializable]
    public class MoveData : IDeepClonable<MoveData>
    {
        [SerializeField] bool _isSet;
        [SerializeField] Direction _directionValue;
        [SerializeField] int _lengthValue;

        public bool IsSet => _isSet;

        public Direction DirectionValue => _directionValue;

        public int LengthValue => _lengthValue;


        public MoveData()
        {
            _isSet = false;
            _directionValue = Direction.None;
            _lengthValue = 0;
        }

        public MoveData(Direction direction, int length)
        {
            _isSet = true;
            _directionValue = direction;
            _lengthValue = length;
        }

        public MoveData DeepClone()
        {
            return new MoveData(_directionValue, _lengthValue);
        }
    }

    public interface ILockable
    {
        void Lock();
    }
}
