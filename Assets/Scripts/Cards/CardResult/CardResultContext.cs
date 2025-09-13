using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards
{
    public enum HandRanking
    {
        None = -1,

        High = 0,
        OnePair = 5,
        TwoPair = 20,
        Triple = 30,
        Straight = 50,
        Flush = 80,
        FourCard = 200,
        StraightFlush = 300  // 스티플
    }


    [Serializable]
    public class CardResultContext
    {
        public readonly MultiplyValues Multiply;

        public CardResult PreviousResult { get; private set; }
        public CardResult CurrentResult { get; private set; }

        public bool IsBlackFlushUsed { get; private set; }

        public CardResultContext(MultiplyValues multiplyValues)
        {
            Multiply = multiplyValues;
        }

        public void GetSet()
        {
            PreviousResult = CurrentResult;
        }

        public void SetResult(CardResult result)
        {
            CurrentResult = result;
        }

        public void SetBlackFlushUsed()
        {
            IsBlackFlushUsed = true;
        }
    }


    /// <summary>
    /// 사용한 카드를 바탕으로 해석된 결과만을 가지는 구조체
    /// </summary>
    [Serializable]
    public readonly struct CardResult
    {
        public readonly float Damage;

        public readonly List<NumberData> Numbers;
        public readonly List<MoveData> Moves;
        public readonly HandRanking Rangking => Rankings[0]; 

        public readonly bool IsRedFlush;
        public readonly bool IsBlueFlush;
        public readonly bool IsGreenFlush;
        public readonly bool IsBlackFlush;

        private readonly CardResultContext Ctx;
        private readonly List<HandRanking> Rankings;
        private readonly List<CardData> CardDatas;
       

        public string Description
        {
            get
            {
                var text = "";

                if (Rankings[0] != HandRanking.None)
                {
                    if (Ctx.IsBlackFlushUsed)
                        text += "[ Black Flush: damage 200% ]\n";

                    if (Ctx.PreviousResult.IsRedFlush)
                        text += "[ Red Flush: damage 300% ]\n";

                    text += $"Ranking: {Rankings[0]}\nDamage: {Damage}\n";
                }

                return text;
            }
        }



        public CardResult(CardResultContext ctx, float damage, List<HandRanking> rankings, List<CardData> cardDatas, List<CardData> numberDatas, List<CardData> moveDatas)
        {
            Ctx = ctx;

            Damage = damage;
            Rankings = rankings;
            CardDatas = cardDatas;

            Numbers = numberDatas.Select(n => n.Number)
                    .ToList();
            Moves = moveDatas.Select(m => m.Move)
                    .ToList();;

            IsRedFlush = IsBlueFlush = IsGreenFlush = IsBlackFlush = false;
            if (rankings.Contains(HandRanking.Flush))
                switch (Numbers[0].ColorValue)
                {
                    case NumberData.CardColor.Red: IsRedFlush = true; break;
                    case NumberData.CardColor.Blue: IsBlueFlush = true; break;
                    case NumberData.CardColor.Green: IsGreenFlush = true; break;
                    case NumberData.CardColor.Black: IsBlackFlush = true; break;
                }
        }
    }
}