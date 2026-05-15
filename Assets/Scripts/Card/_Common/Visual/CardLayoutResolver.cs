using Cardevil.Card.Common.Core;
using System;
using System.Linq;

namespace Cardevil.Card.Common.Visual
{
    public static class CardLayoutResolver
    {
        public static CardLayoutData Resolve(in CardVisualInput input)
        {
            return input.Type switch
            {
                CardType.Attack => ResolveAttack(input),
                CardType.Move => ResolveMove(input),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static CardLayoutData ResolveAttack(in CardVisualInput input)
        {
            var color = input.CurrentColor;

            if (input.NumberOptions.Length > 1 && !input.NumberSelected)
            {
                if (input.NumberOptions.Length == 9)
                {
                    var innerFrame = CardSpriteCache.GetInnerFrame(color);
                    var mainSprite = CardSpriteCache.GetStar(color);

                    return CardLayoutData.Single(innerFrame, mainSprite);
                }

                if (input.NumberOptions.Length == 2 || input.NumberOptions.Length == 3)
                {
                    var innerFrame = CardSpriteCache.GetInnerFrame(color);
                    var sprites = input.NumberOptions
                        .Select(n => CardSpriteCache.GetNumber(color, n))
                        .ToArray();
                    
                    if (sprites.Length == 2)
                    {
                        return CardLayoutData.Dual(innerFrame, sprites[0], sprites[1]);
                    }
                    if (sprites.Length == 3)
                    {
                        return CardLayoutData.Triple(innerFrame, sprites[0], sprites[1], sprites[2]);
                    }
                }
            }
            
            // 숫자 한개, 선택 완료 등 나머지 -> 단일 레이아웃
            {
                var number = input.CurrentNumber;
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var mainSprite = CardSpriteCache.GetNumber(color, number);
                var cornerSprite = CardSpriteCache.GetSmallNumber(color, number);

                return CardLayoutData.SingleWithCorner(innerFrame, mainSprite, cornerSprite);
            }
        }

        private static CardLayoutData ResolveMove(in CardVisualInput input)
        {
            // 방향 여러개 + 미선택 -> 다중 화살표
            if (input.DirectionOptions.Length > 1 && !input.DirectionSelected)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(input.DirectionFlag);
                var mainSprite = CardSpriteCache.GetArrow(input.DirectionFlag);

                return CardLayoutData.Single(innerFrame, mainSprite);
            }

            // 방향 한개, 선택 완료 등 나머지 -> 단일 화살표
            {
                var direction = input.CurrentDirection;
                var innerFrame = CardSpriteCache.GetInnerFrame(direction);
                var mainSprite = CardSpriteCache.GetArrow(direction);

                return CardLayoutData.Single(innerFrame, mainSprite);
            }
        }
    }
}
