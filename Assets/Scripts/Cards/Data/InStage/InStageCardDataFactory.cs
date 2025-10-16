using Cardevil.Utils;
using System.Collections.Generic;

namespace Cardevil.Cards.Data.InStage
{
    public static class InStageCardDataFactory
    {
        public static List<InStageCardData> BuildInStageCardData(List<CardData> data)
        {
            List<InStageCardData> builtDatas = new();

            foreach (CardData origin in data)
            {
                InStageCardData builtData;
                if (origin.NumberModifiers != null)
                {
                    var builtNumber = origin.NumberModifiers.Build();
                    builtData = InStageCardData.FromNumber(origin.Id, builtNumber);
                    builtDatas.Add(builtData);
                }
                else if (origin.MoveModifiers != null)
                {
                    var builtMove = origin.MoveModifiers.Build();
                    builtData = InStageCardData.FromMove(origin.Id, builtMove);
                    builtDatas.Add(builtData);
                }
                
                else
                {
                    LogEx.LogError("잘못된 Modifier을 가지고 있는 CardData!");
                }
            }
            
            return builtDatas;
        }
    }
}