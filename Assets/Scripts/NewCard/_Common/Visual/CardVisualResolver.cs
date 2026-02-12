using Cardevil.NewCard.Common.Core;
using System;
using System.Linq;

namespace Cardevil.NewCard.Common.Visual
{
    public static class CardVisualResolver
    {
        public static CardVisualData Resolve(in CardVisualInput input)
        {
            return input.Type switch
            {
                CardType.Attack => ResolveAttack(input),
                CardType.Move => ResolveMove(input),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static CardVisualData ResolveAttack(in CardVisualInput input)
        {
            var color = input.Color;

            // 오망성
            if (input.NumberOptions.Length == 9)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var mainSprite = CardSpriteCache.GetStar(color);

                return CardVisualData.Single(innerFrame, mainSprite);
            }

            // 숫자 여러개 + 미선택 -> 복수 레이아웃
            if (input.NumberOptions.Length > 1 && !input.NumberSelected)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var sprites = input.NumberOptions
                    .Select(n => CardSpriteCache.GetNumber(color, n))
                    .ToArray();

                if (sprites.Length == 2)
                {
                    return CardVisualData.Dual(innerFrame, sprites[0], sprites[1]);
                }
                else
                {
                    return CardVisualData.Triple(innerFrame, sprites[0], sprites[1], sprites[2]);
                }
            }

            // 숫자 한개, 선택 완료 등 나머지 -> 단일 레이아웃
            {
                var number = input.CurrentNumber;
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var mainSprite = CardSpriteCache.GetNumber(color, number);
                var cornerSprite = CardSpriteCache.GetSmallNumber(color, number);

                return CardVisualData.SingleWithCorner(innerFrame, mainSprite, cornerSprite);
            }
        }

        private static CardVisualData ResolveMove(in CardVisualInput input)
        {
            // 방향 여러개 + 미선택 -> 다중 화살표
            if (input.DirectionOptionsCount > 1 && !input.DirectionSelected)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(input.DirectionFlag);
                var mainSprite = CardSpriteCache.GetArrow(input.DirectionFlag);

                return CardVisualData.Single(innerFrame, mainSprite);
            }

            // 방향 한개, 선택 완료 등 나머지 -> 단일 화살표
            {
                var direction = input.CurrentDirection;
                var innerFrame = CardSpriteCache.GetInnerFrame(direction);
                var mainSprite = CardSpriteCache.GetArrow(direction);

                return CardVisualData.Single(innerFrame, mainSprite);
            }
        }
    }
}
