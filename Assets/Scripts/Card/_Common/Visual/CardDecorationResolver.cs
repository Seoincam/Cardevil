using System.Linq;

namespace Cardevil.Card.Common.Visual
{
    public static class CardDecorationResolver
    {
        public static CardDecorationData Resolve(in CardVisualInput input)
        {
            if (input.ColorOptions == null || input.ColorOptions.Length <= 1)
            {
                return CardDecorationData.Empty;
            }
            
            var decorations = CardDecorations.ColorJewel;

            if (input.ColorOptions.Length == 4)
            {
                var jewelSprite = CardSpriteCache.GetColorJewel("Prism");
                return new CardDecorationData(decorations, jewelSprite);
            }
                
            var jewelSprites = input.ColorOptions
                .Select(c => CardSpriteCache.GetColorJewel(c.ToString()))
                .ToArray();
            return new CardDecorationData(decorations, jewelSprites);
        }
    }
}