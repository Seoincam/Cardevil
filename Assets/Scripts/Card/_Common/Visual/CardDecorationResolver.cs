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

            // 과거의 잔재. 원래는 강화 프레임도 처리할 예정이었으나, 현재는 색깔 보석만 남았음.
            var decorationType = CardDecorationType.ColorJewel;

            if (input.AllColorCandidates.Length == 4)
            {
                var jewelSprite = CardSpriteCache.GetColorJewel("Prism");
                return new CardDecorationData(decorationType, jewelSprite);
            }
                
            var jewelSprites = input.AllColorCandidates
                .Select(c =>
                {
                    if (c.HasValue)
                    {
                        return CardSpriteCache.GetColorJewel(c.Value.ToString());
                    }

                    // 색이 지정되지 않았을 경우 하얀색으로.
                    return CardSpriteCache.GetColorJewel("White");
                }).ToArray();
            
            return new CardDecorationData(decorationType, jewelSprites);
        }
    }
}