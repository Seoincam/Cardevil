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
            var baseColor = input.AllColorCandidates[0];
            var currentColor = input.CurrentColor;

            if (input.AllNumberCandidates.Length > 1 && !input.CurrentNumber.HasValue && currentColor.HasValue)
            {
                if (input.AllNumberCandidates.Length == 9)
                {
                    var innerFrame = CardSpriteCache.GetInnerFrame(currentColor.Value);
                    var mainSprite = CardSpriteCache.GetStar(currentColor.Value);

                    return CardLayoutData.Single(innerFrame, mainSprite);
                }

                if (input.AllNumberCandidates.Length == 2 || input.AllNumberCandidates.Length == 3)
                {
                    var innerFrame = CardSpriteCache.GetInnerFrame(currentColor.Value);
                    var sprites = input.AllNumberCandidates
                        .Select(n =>
                        {
                            if (n.HasValue)
                                return CardSpriteCache.GetNumber(currentColor.Value, n.Value);
                            else
                                return CardSpriteCache.GetQuestionMark(currentColor.Value);
                        })
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
                var innerFrame = CardSpriteCache.GetInnerFrame(baseColor.Value);
                var mainSprite = CardSpriteCache.GetNumber(baseColor.Value, number.Value);
                var cornerSprite = CardSpriteCache.GetSmallNumber(baseColor.Value, number.Value);

                return CardLayoutData.SingleWithCorner(innerFrame, mainSprite, cornerSprite);
            }
        }

        private static CardLayoutData ResolveMove(in CardVisualInput input)
        {
            // 방향 여러개 + 미선택 -> 다중 화살표
            if (input.AllDirectionCandidates.Length > 1 && !input.CurrentDirection.HasValue)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(input.DirectionFlag);
                var mainSprite = CardSpriteCache.GetArrow(input.DirectionFlag);

                return CardLayoutData.Single(innerFrame, mainSprite);
            }

            // 방향 한개, 선택 완료 등 나머지 -> 단일 화살표
            {
                var direction = input.CurrentDirection;
                var innerFrame = CardSpriteCache.GetInnerFrame(input.CurrentDirection.Value);
                var mainSprite = CardSpriteCache.GetArrow(input.CurrentDirection.Value);

                return CardLayoutData.Single(innerFrame, mainSprite);
            }
        }
    }
}
