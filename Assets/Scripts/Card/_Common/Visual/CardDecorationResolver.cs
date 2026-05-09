using System.Linq;

namespace Cardevil.Card.Common.Visual
{
    public static class CardDecorationResolver
    {
        public static CardDecorationData Resolve(in CardVisualInput input)
        {
            if (input.AllColorCandidates == null || input.AllColorCandidates.Length <= 1)
            {
                return CardDecorationData.Empty;
            }
            
            var decorations = CardDecorations.ColorJewel;

            if (input.AllColorCandidates.Length == 4)
            {
                var jewelSprite = CardSpriteCache.GetColorJewel("Prism");
                return new CardDecorationData(decorations, jewelSprite);
            }
                
            var jewelSprites = input.AllColorCandidates
                .Select(c => CardSpriteCache.GetColorJewel(c.ToString()))
                .ToArray();
            return new CardDecorationData(decorations, jewelSprites);
        }
    }
}