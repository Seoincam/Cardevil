using Cardevil.Card.Common.Core;
using Cardevil.Core.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Visual
{
    /// <summary>
    /// Visual 로직들이 CardState에 종속되지 않도록 하는 경량 구조체.
    /// State 없이도 생성 가능함.
    /// </summary>
    [Serializable]
    public struct CardVisualInput
    {
        [field: SerializeField] public CardType Type { get; private set; }
        
        [field: Header("Attack (Color)")]
        [field: SerializeField] public Optional<CardColor> BaseColor { get; private set; }
        [field: SerializeField] public Optional<CardColor>[] AllColorCandidates { get; private set; }
        [field: SerializeField] public Optional<CardColor> FixedColor { get; private set; }

        [field: Header("Attack (Number)")]
        [field: SerializeField] public Optional<int>[] AllNumberCandidates { get; private set; }
        [field: SerializeField] public Optional<int> FixedNumber { get; private set; }

        [field: Header("Move")]
        [field: SerializeField] public Optional<Direction> FixedDirection { get; private set; }
        [field: SerializeField] public Direction[] AllDirectionCandidates { get; private set; }
        [field: SerializeField] public DirectionFlag DirectionFlag { get; private set; }


        private CardVisualInput(
            CardType type,
            CardColor? baseColor,
            CardColor?[] allColorCandidates,
            CardColor? fixedColor,
            int?[] allNumberCandidates,
            int? fixedNumber,
            Direction[] allDirectionCandidates,
            Direction? fixedDirection,
            DirectionFlag directionFlag 
        )
        {
            Type = type;
            BaseColor = new Optional<CardColor>(baseColor);
            AllColorCandidates = allColorCandidates?.Select(c => new Optional<CardColor>(c)).ToArray();
            FixedColor = new Optional<CardColor>(fixedColor);
            AllNumberCandidates = allNumberCandidates?.Select(n => new Optional<int>(n)).ToArray();
            FixedNumber = new Optional<int>(fixedNumber);
            AllDirectionCandidates = allDirectionCandidates;
            FixedDirection = new Optional<Direction>(fixedDirection);
            DirectionFlag = directionFlag;
        }

        public static CardVisualInput From(CardSpec spec)
        {
            return From(spec.State);
        }

        public static CardVisualInput From(ICardState state)
        {
            CardColor?[] allColorCandidates = null;
            CardColor? fixedColor = null;

            int?[] allNumberCandidates = null;
            int? fixedNumber = null;

            Direction[] allDirectionCandidates = null;
            Direction? fixedDirection = null;
            DirectionFlag directionFlag = DirectionFlag.None;
            
            if (state.ColorList != null && state.ColorList.IsInitialized)
            {
                allColorCandidates = state.ColorList.AllCandidateValues.ToArray();
                fixedColor = state.ColorList.IsFixed ? state.ColorList.FixedValue : null;
            }

            if (state.NumberList != null && state.NumberList.IsInitialized)
            {
                allNumberCandidates = state.NumberList.AllCandidateValues.ToArray();
                fixedNumber = state.NumberList.IsFixed ? state.NumberList.FixedValue : null;
            }

            if (state.DirectionList != null && state.DirectionList.IsInitialized)
            {
                allDirectionCandidates = state.DirectionList.AllCandidateValues.Select(d => d!.Value).ToArray();
                fixedDirection = state.DirectionList.IsFixed ? state.DirectionList.FixedValue : null;
                directionFlag = state.DirectionFlag;
            }

            return new CardVisualInput(
                state.Type,
                state.BaseColor,
                allColorCandidates,
                fixedColor,
                allNumberCandidates,
                fixedNumber,
                allDirectionCandidates,
                fixedDirection,
                directionFlag);
        }
        
        public static CardVisualInput Attack(CardColor color, params int?[] numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                baseColor: color,
                allColorCandidates: new CardColor?[] { color },
                fixedColor: color,
                allNumberCandidates: numbers,
                fixedNumber: numbers.Length > 0 ? numbers[0] : 0,
                fixedDirection: Direction.None,
                allDirectionCandidates: null,
                directionFlag: DirectionFlag.None
            );
        }

        public static CardVisualInput Attack(CardColor baseColor, CardColor?[] colors, int numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                baseColor,
                allColorCandidates: colors,
                fixedColor: colors.Length > 0 ? colors[0] : CardColor.None,
                allNumberCandidates: new int?[] { numbers },
                fixedNumber: numbers,
                fixedDirection: Direction.None,
                allDirectionCandidates: null,
                directionFlag: DirectionFlag.None
            );
        }

        public static CardVisualInput Move(Direction direction)
        {
            return new CardVisualInput(
                type: CardType.Move,
                baseColor: null,
                allColorCandidates: null,
                fixedColor: CardColor.None,
                allNumberCandidates: null,
                fixedNumber: 0,
                fixedDirection: direction,
                allDirectionCandidates: new[] { direction },
                directionFlag: direction.ToDirectionFlag()
            );
        }

        public static CardVisualInput Move(DirectionFlag flag, params Direction[] directions)
        {
            return new CardVisualInput(
                type: CardType.Move,
                baseColor: null,
                allColorCandidates: null,
                fixedColor: CardColor.None,
                allNumberCandidates: null,
                fixedNumber: 0,
                fixedDirection: directions.Length > 0 ? directions[0] : Direction.None,
                allDirectionCandidates: directions,
                directionFlag: flag
            );
        }
    }
}
