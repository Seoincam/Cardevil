namespace Cardevil.NewCard.Common.Visual
{
    public enum CardLayoutType : byte
    {
        Single,
        SingleWithCorner,
        Dual,
        Triple
    }

    public readonly struct CardVisualData
    {
        public readonly CardLayoutType Type;
        public readonly SpriteReference InnerFrame;
        public readonly SpriteReference MainSprite;
        public readonly SpriteReference CornerSprite;
        public readonly SpriteReference[] SubSprites;
        
        public static CardVisualData Single(SpriteReference frame, SpriteReference main)
            => new(CardLayoutType.Single, frame, main, default, null);

        public static CardVisualData SingleWithCorner(SpriteReference frame, SpriteReference main, SpriteReference corner)
            => new(CardLayoutType.SingleWithCorner, frame, main, corner, null);

        public static CardVisualData Dual(SpriteReference frame, SpriteReference subs1, SpriteReference subs2)
            => new(CardLayoutType.Dual, frame, default, default, subs1, subs2);
        
        public static CardVisualData Triple(SpriteReference frame, SpriteReference subs1, SpriteReference subs2, SpriteReference subs3)
            => new(CardLayoutType.Triple, frame, default, default, subs1, subs2, subs3);

        private CardVisualData(
            CardLayoutType type, 
            SpriteReference innerFrame, 
            SpriteReference main, 
            SpriteReference corner,
            params SpriteReference[] subSprites)
        {
            Type = type;
            InnerFrame = innerFrame;
            MainSprite = main;
            CornerSprite = corner;
            SubSprites = subSprites;
        }
    }
}
