using Cardevil.Cards.Visual.StateMachine;

namespace Cardevil.Cards.Visual.Sprites
{
    public readonly struct CardSpriteSet
    {
        public readonly SpriteKey InnerFrame;
        public readonly SpriteKey[] MainSprites;
        public readonly SpriteKey SmallNumber;
        
        public SpriteKey Primary => MainSprites[0];
        public VisualPhase Phase => (VisualPhase)MainSprites.Length;

        public bool HasSmallNumber => SmallNumber.IsValid;
        
        private CardSpriteSet(SpriteKey innerFrame, SpriteKey[] mainSprites, SpriteKey smallNumber)
        {
            InnerFrame = innerFrame;
            MainSprites = mainSprites;
            SmallNumber = smallNumber;
        }

        public static CardSpriteSet Single(SpriteKey innerFrame, SpriteKey mainSprite) =>
            new(innerFrame, new[] { mainSprite }, SpriteKey.Empty);
        
        public static CardSpriteSet SingleWithSmall(
            SpriteKey innerFrame, 
            SpriteKey mainSprite, 
            SpriteKey smallNumber) 
            => new (innerFrame, new[] { mainSprite }, smallNumber);

        public static CardSpriteSet Multiple(SpriteKey innerFrame, params SpriteKey[] mainSprites)
            => new(innerFrame, mainSprites, SpriteKey.Empty);
    }
}