using Cardevil.Card.Common.Core;
using Cardevil.Core.Utils;
using System.Linq;

namespace Cardevil.Card.Common.Visual
{
    /// <summary>
    /// Visual 로직들이 CardState에 종속되지 않도록 하는 경량 구조체.
    /// State 없이도 생성 가능함.
    /// </summary>
    public readonly struct CardVisualInput
    {
        public readonly CardType Type;

        // Attack - Color
        public readonly CardColor?[] AllColorCandidates;
        public readonly CardColor? CurrentColor;

        // Attack - Number
        public readonly int?[] AllNumberCandidates;
        public readonly int? CurrentNumber;

        // Move
        public readonly Direction? CurrentDirection;
        public readonly Direction[] AllDirectionCandidates;
        public readonly DirectionFlag DirectionFlag;


        private CardVisualInput(
            CardType type,
            CardColor?[] allColorCandidates,
            CardColor? currentColor,
            int?[] allNumberCandidates,
            int? currentNumber,
            Direction[] allDirectionCandidates,
            Direction? currentDirection,
            DirectionFlag directionFlag 
        )
        {
            Type = type;
            AllColorCandidates = allColorCandidates;
            CurrentColor = currentColor;
            AllNumberCandidates = allNumberCandidates;
            CurrentNumber = currentNumber;
            AllDirectionCandidates = allDirectionCandidates;
            CurrentDirection = currentDirection;
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
                allDirectionCandidates = state.DirectionList.AllCandidateValues.Select(d => d.Value).ToArray();
                fixedDirection = state.DirectionList.IsFixed ? state.DirectionList.FixedValue : null;
                directionFlag = state.DirectionFlag;
            }

            return new CardVisualInput(
                state.Type,
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
                allColorCandidates: new CardColor?[] { color },
                currentColor: color,
                allNumberCandidates: numbers,
                currentNumber: numbers.Length > 0 ? numbers[0] : 0,
                currentDirection: Direction.None,
                allDirectionCandidates: null,
                directionFlag: DirectionFlag.None
            );
        }

        public static CardVisualInput Attack(CardColor?[] colors, int numbers)
        {
            return new CardVisualInput(
                type: CardType.Attack,
                allColorCandidates: colors,
                currentColor: colors.Length > 0 ? colors[0] : CardColor.None,
                allNumberCandidates: new int?[] { numbers },
                currentNumber: numbers,
                currentDirection: Direction.None,
                allDirectionCandidates: null,
                directionFlag: DirectionFlag.None
            );
        }

        public static CardVisualInput Move(Direction direction)
        {
            return new CardVisualInput(
                type: CardType.Move,
                allColorCandidates: null,
                currentColor: CardColor.None,
                allNumberCandidates: null,
                currentNumber: 0,
                currentDirection: direction,
                allDirectionCandidates: new[] { direction },
                directionFlag: direction.ToDirectionFlag()
            );
        }

        public static CardVisualInput Move(DirectionFlag flag, params Direction[] directions)
        {
            return new CardVisualInput(
                type: CardType.Move,
                allColorCandidates: null,
                currentColor: CardColor.None,
                allNumberCandidates: null,
                currentNumber: 0,
                currentDirection: directions.Length > 0 ? directions[0] : Direction.None,
                allDirectionCandidates: directions,
                directionFlag: flag
            );
        }
    }
}
