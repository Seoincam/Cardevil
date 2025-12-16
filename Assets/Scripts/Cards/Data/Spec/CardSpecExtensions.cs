using Cardevil.Cards.Data.InStage;
using Cardevil.Utils;
using System;

namespace Cardevil.Cards.Data.Spec
{
    public static class CardSpecExtensions
    {
        public static CardData Build(this IReadOnlyCardSpec spec)
        {
            var builder = CardData.CreateBuilder(spec.Id, spec.Kind);
            
            foreach (var modifier in spec.Modifiers)
                modifier.Apply(builder);
            
            try
            {
                var cardData = builder.Build();
                cardData.Validate();

                return cardData;
            }
            catch (Exception ex)
            {
                LogEx.LogError($"유효하지 않은 Card Data 생성이 시도됨. ({spec.Id}): {ex.Message}");
            }

            return null;
        }
    }
}