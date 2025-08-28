using Cardevil.Cards.CardInteractinos;
using Cardevil.Utils.Directions;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Cardevil.Cards
{
    [Serializable]
    public sealed class CardData : ILockable
    {
        [Header("Card")]
        public CardType type;
        public enum CardType { Number, Move }

        public NumberData Number;
        public MoveData Move;

        public bool isLocked = false;

        [Header("Select")]
        public List<int> numberOptions;
        public List<Direction> directionOptions;

        /// <summary>
        /// 값 선택 완료 여부를 반환.
        /// </summary>
        public bool IsValueSelected
        {
            get => type == CardType.Number ? Number.number != 0 : (Move.direction != Direction.None && Move.length > 0);
        }

        /// <summary>
        /// 값 선택 가능 여부를 반환.
        /// </summary>
        public bool CanOpenSelection
        {
            get => !isLocked && (CanSelectNumber || CanSelectDirection);
        }

        private bool CanSelectNumber
        {
            get => numberOptions?.Count > 0;
        }

        private bool CanSelectDirection
        {
            get => directionOptions?.Count > 0;
        }

        [Header("Reinforce")]
        [Tooltip("카드가 강화 가능한가?")]
        public bool reinforceEnabled = true;


        // 선택
        public bool OpenSelection(Card card, SelectContainer container)
        {
            if (!CanOpenSelection)
                return false;

            container.Init(card);

            if (CanSelectNumber)
                foreach (var number in numberOptions)
                    container.AddOption(number);
            if (CanSelectDirection)
                foreach (var direction in directionOptions)
                    container.AddOption(direction);

            container.OpenSelection();
            return true;
        }


        // 강화
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

        public void Lock()
        {
            isLocked = true;
        }

        public CardData Copy()
        {
            return new CardData()
            {
                type = type,
                isLocked = false,
                Number = new() { color = Number.color, number = Number.number },
                Move = new() { direction = Move.direction, length = Move.length },
                numberOptions = new(numberOptions),
                directionOptions = new(directionOptions),
                reinforceEnabled = reinforceEnabled
            };
        }
    }



    [Serializable]
    public class NumberData
    {
        public enum CardColor { Red, Blue, Green, Black }
        public CardColor color;
        public int number;
    }

    [Serializable]
    public class MoveData
    {
        public Direction direction;
        public int length;
    }

    public interface ILockable
    {
        void Lock();
    }

    public enum HandRanking
    {
        None = -1,

        High = 0,
        OnePair = 5,
        TwoPair = 20,
        Triple = 30,
        Straight = 50,
        Flush = 80,
        FourCard = 200,
        StraightFlush = 300  // 스티플
    }
}
