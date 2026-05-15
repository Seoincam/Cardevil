using System;

namespace Cardevil.Card.Common.Visual
{
    [Flags]
    public enum CardDecorationType : byte
    {
        None = 0,
        
        /// <summary>
        /// 다중 색을 가진 카드의 표시 보석.
        /// </summary>
        ColorJewel = 1 << 0,
        
        /// <summary>
        /// 데미지 증가를 가진 카드의 표시 프레임.
        /// </summary>
        DamageFrame = 1 << 1
    }
    
    public readonly struct CardDecorationData
    {
        public readonly CardDecorationType DecorationType;
        public readonly SpriteReference[] JewelSprites;
        
        public static CardDecorationData Empty => new CardDecorationData(CardDecorationType.None, null);

        public CardDecorationData(CardDecorationType decorationType, params SpriteReference[] jewelSprites)
        {
            DecorationType = decorationType;
            JewelSprites = jewelSprites;
        }
    }
}