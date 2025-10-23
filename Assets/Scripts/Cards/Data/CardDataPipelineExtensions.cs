using Cardevil.Cards.Data.InStage;
using Cardevil.Utils;
using System;

namespace Cardevil.Cards.Data
{
    public static class CardDataPipelineExtensions
    {
        public static CardData Build(this IReadOnlyCardDataPipeline pipeline)
        {
            var builder = CardData.CreateBuilder(pipeline.Id, pipeline.Kind);
            
            foreach (var modifier in pipeline.Modifiers)
                modifier.Apply(builder);
            
            try
            {
                var cardData = builder.Build();
                cardData.Validate();

                return cardData;
            }
            catch (Exception ex)
            {
                LogEx.LogError($"유효하지 않은 Card Data 생성이 시도됨. ({pipeline.Id}): {ex.Message}");
            }

            return null;
        }
    }
}