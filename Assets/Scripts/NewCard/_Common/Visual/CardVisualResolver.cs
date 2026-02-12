using Cardevil.NewCard.Common.Core;
using System;
using System.Linq;

namespace Cardevil.NewCard.Common.Visual
{
    public static class CardVisualResolver
    {
        public static CardVisualData Resolve(CardState state)
        {
            return state.Type switch
            {
                CardType.Attack => ResolveAttack(state),
                CardType.Move => ResolveMove(state),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    
        private static CardVisualData ResolveAttack(CardState state)
        {
            var color = state.Colors.Current ?? state.Colors.DefaultValue;
            
            // 오망성
            if (state.Numbers.AllOptionsCount == 9)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var mainSprite = CardSpriteCache.GetStar(color);
                
                return CardVisualData.Single(innerFrame, mainSprite);
            }
    
            // 숫자 여러개 + 미선택 -> 복수 레이아웃
            if (state.Numbers.AllOptionsCount > 1 && !state.Numbers.HasSelected)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var sprites = state.Numbers.AllOptions
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
                var number = state.Numbers.Current ?? state.Numbers.DefaultValue;
                var innerFrame = CardSpriteCache.GetInnerFrame(color);
                var mainSprite = CardSpriteCache.GetNumber(color, number);
                var cornerSprite = CardSpriteCache.GetSmallNumber(color, number);
                
                return CardVisualData.SingleWithCorner(innerFrame, mainSprite, cornerSprite);
            }
        }
    
        private static CardVisualData ResolveMove(CardState state)
        {
            // 방향 여러개 + 미선택 -> 다중 화살표
            if (state.Directions.AllOptionsCount > 1 && !state.Directions.HasSelected)
            {
                var innerFrame = CardSpriteCache.GetInnerFrame(state.DirectionFlag);
                var mainSprite = CardSpriteCache.GetArrow(state.DirectionFlag);
                
                return CardVisualData.Single(innerFrame, mainSprite);
            }
    
            // 방향 한개, 선택 완료 등 나머지 -> 단일 화살표
            {
                var direction = state.Directions.Current ?? state.Directions.DefaultValue;
                var innerFrame = CardSpriteCache.GetInnerFrame(direction);
                var mainSprite = CardSpriteCache.GetArrow(direction);
                
                return CardVisualData.Single(innerFrame, mainSprite);
            }
        }
    }
}