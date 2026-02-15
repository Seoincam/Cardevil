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

        // Attack - Color
        public readonly CardColor[] ColorOptions;
        public readonly CardColor CurrentColor;
        public readonly bool ColorSelected;

        // Attack - Number
        public readonly int[] NumberOptions;
        public readonly int CurrentNumber;
        public readonly bool NumberSelected;

        // Move
        public readonly Direction CurrentDirection;
        public readonly Direction[] DirectionOptions;
        public readonly DirectionFlag DirectionFlag;
        public readonly bool DirectionSelected;

        private CardVisualInput(
            CardType type,
            CardColor[] colorOptions,
            CardColor currentColor,
            bool colorSelected,
            int[] numberOptions,
            int currentNumber,
            bool numberSelected,
            Direction currentDirection,
            Direction[] directionOptions,
            DirectionFlag directionFlag,
            bool directionSelected)
        {
            Type = type;
            ColorOptions = colorOptions;
            CurrentColor = currentColor;
            ColorSelected = colorSelected;
            NumberOptions = numberOptions;
            CurrentNumber = currentNumber;
            NumberSelected = numberSelected;
            CurrentDirection = currentDirection;
            DirectionOptions = directionOptions;
            DirectionFlag = directionFlag;
            DirectionSelected = directionSelected;
        }

        public static CardVisualInput From(ICardState state)
        {
            CardColor[] colorOptions = null;
            var currentColor = CardColor.None;
            var colorSelected = false;
            int[] numberOptions = null;
            var currentNumber = 0;
            var numberSelected = false;

            if (state.Colors is { Initialized: true })
            {
                colorOptions = state.Colors.AllOptions.ToArray();
                currentColor = state.Colors.Current ?? state.Colors.DefaultValue;
                colorSelected = state.Colors.HasSelected;
            }
            if (state.Numbers is { Initialized: true})
            {
                numberOptions = state.Numbers.AllOptions.ToArray();
                currentNumber = state.Numbers.Current ?? state.Numbers.DefaultValue;
                numberSelected = state.Numbers.HasSelected;
            }

            var currentDirection = Direction.None;
            Direction[] directionOptions = null;
            var directionSelected = false;

            if (state.Directions is { Initialized: true })
            {
                currentDirection = state.Directions.Current ?? state.Directions.DefaultValue;
                directionOptions = state.Directions.AllOptions.ToArray();
                directionSelected = state.Directions.HasSelected;
            }

            return new CardVisualInput(
                type: state.Type,
                colorOptions: colorOptions,
                currentColor: currentColor,
                colorSelected: colorSelected,
                numberOptions: numberOptions,
                currentNumber: currentNumber,
                numberSelected: numberSelected,
                currentDirection: currentDirection,
                directionFlag: state.DirectionFlag,
                directionOptions: directionOptions,
                directionSelected: directionSelected
            );
        }

        public static CardVisualInput Attack(CardColor color, params int[] numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                colorOptions: new[] { color },
                currentColor: color,
                colorSelected: true,
                numberOptions: numbers,
                currentNumber: numbers.Length > 0 ? numbers[0] : 0,
                numberSelected: numbers.Length <= 1,
                currentDirection: Direction.None,
                directionOptions: null,
                directionFlag: DirectionFlag.None,
                directionSelected: false
            );
        }

        public static CardVisualInput Attack(CardColor[] colors, int numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                colorOptions: colors,
                currentColor: colors.Length > 0 ? colors[0] : CardColor.None,
                colorSelected: colors.Length <= 1,
                numberOptions: new[] { numbers },
                currentNumber: numbers,
                numberSelected: true,
                currentDirection: Direction.None,
                directionOptions: null,
                directionFlag: DirectionFlag.None,
                directionSelected: false
            );
        }

        public static CardVisualInput Move(Direction direction)
        {
            return new CardVisualInput(
                type: CardType.Move,
                colorOptions: null,
                currentColor: CardColor.None,
                colorSelected: false,
                numberOptions: null,
                currentNumber: 0,
                numberSelected: false,
                currentDirection: direction,
                directionOptions: new[] { direction },
                directionFlag: direction.ToDirectionFlag(),
                directionSelected: true
            );
        }

        public static CardVisualInput Move(DirectionFlag flag, params Direction[] directions)
        {
            return new CardVisualInput(
                type: CardType.Move,
                colorOptions: null,
                currentColor: CardColor.None,
                colorSelected: false,
                numberOptions: null,
                currentNumber: 0,
                numberSelected: false,
                currentDirection: directions.Length > 0 ? directions[0] : Direction.None,
                directionOptions: directions,
                directionFlag: flag,
                directionSelected: false
            );
        }
    }
}
