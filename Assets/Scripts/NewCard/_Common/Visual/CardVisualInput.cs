using Cardevil.NewCard.Common.Core;
using Cardevil.Utils.Directions;
using System.Linq;

namespace Cardevil.NewCard.Common.Visual
{
    /// <summary>
    /// Visual 로직들이 CardState에 종속되지 않도록 하는 경량 구조체.
    /// State 없이도 생성 가능함.
    /// </summary>
    public readonly struct CardVisualInput
    {
        public readonly CardType Type;

        // Attack
        public readonly CardColor Color;
        public readonly int[] NumberOptions;
        public readonly int CurrentNumber;
        public readonly bool NumberSelected;

        // Move
        public readonly Direction CurrentDirection;
        public readonly DirectionFlag DirectionFlag;
        public readonly int DirectionOptionsCount;
        public readonly bool DirectionSelected;

        private CardVisualInput(
            CardType type,
            CardColor color,
            int[] numberOptions,
            int currentNumber,
            bool numberSelected,
            Direction currentDirection,
            DirectionFlag directionFlag,
            int directionOptionsCount,
            bool directionSelected)
        {
            Type = type;
            Color = color;
            NumberOptions = numberOptions;
            CurrentNumber = currentNumber;
            NumberSelected = numberSelected;
            CurrentDirection = currentDirection;
            DirectionFlag = directionFlag;
            DirectionOptionsCount = directionOptionsCount;
            DirectionSelected = directionSelected;
        }

        public static CardVisualInput From(CardState state)
        {
            var color = CardColor.None;
            int[] numberOptions = null;
            var currentNumber = 0;
            var numberSelected = false;

            if (state.Colors != null)
                color = state.Colors.Current ?? state.Colors.DefaultValue;
            if (state.Numbers != null)
            {
                numberOptions = state.Numbers.AllOptions.ToArray();
                currentNumber = state.Numbers.Current ?? state.Numbers.DefaultValue;
                numberSelected = state.Numbers.HasSelected;
            }

            var currentDirection = Direction.None;
            var directionOptionsCount = 0;
            var directionSelected = false;

            if (state.Directions != null)
            {
                currentDirection = state.Directions.Current ?? state.Directions.DefaultValue;
                directionOptionsCount = state.Directions.AllOptionsCount;
                directionSelected = state.Directions.HasSelected;
            }

            return new CardVisualInput(
                type: state.Type,
                color: color,
                numberOptions: numberOptions,
                currentNumber: currentNumber,
                numberSelected: numberSelected,
                currentDirection: currentDirection,
                directionFlag: state.DirectionFlag,
                directionOptionsCount: directionOptionsCount,
                directionSelected: directionSelected
            );
        }

        public static CardVisualInput Attack(CardColor color, params int[] numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                color: color,
                numberOptions: numbers,
                currentNumber: numbers.Length > 0 ? numbers[0] : 0,
                numberSelected: numbers.Length <= 1,
                currentDirection: Direction.None,
                directionFlag: DirectionFlag.None,
                directionOptionsCount: 0,
                directionSelected: false
            );
        }

        public static CardVisualInput Move(Direction direction)
        {
            return new CardVisualInput(
                type: CardType.Move,
                color: CardColor.None,
                numberOptions: null,
                currentNumber: 0,
                numberSelected: false,
                currentDirection: direction,
                directionFlag: direction.ToDirectionFlag(),
                directionOptionsCount: 1,
                directionSelected: true
            );
        }

        public static CardVisualInput Move(DirectionFlag flag, params Direction[] directions)
        {
            return new CardVisualInput(
                type: CardType.Move,
                color: CardColor.None,
                numberOptions: null,
                currentNumber: 0,
                numberSelected: false,
                currentDirection: directions.Length > 0 ? directions[0] : Direction.None,
                directionFlag: flag,
                directionOptionsCount: directions.Length,
                directionSelected: false
            );
        }
    }
}
