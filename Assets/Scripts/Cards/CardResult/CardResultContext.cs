using System;
using System.Collections.Generic;

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
    public class CardContext
    {
        public readonly MultiplyValues Multiply;

        public CardResult PreviousResult { get; private set; }
        public CardResult CurrentResult { get; private set; }

        public bool IsBlackFlushUsed { get; private set; }

        public CardContext(MultiplyValues multiplyValues)
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
        public readonly List<MoveData> Moves;

        public readonly bool IsRedFlush;
        public readonly bool IsBlueFlush;
        public readonly bool IsGreenFlush;
        public readonly bool IsBlackFlush;

        private readonly CardContext ctx;
        private readonly List<HandRanking> Rankings;

        public readonly HandRanking Rangking => Rankings[0];        

        public string Description
        {
            get
            {
                var text = "";

                if (Rankings[0] != HandRanking.None)
                {
                    if (ctx.IsBlackFlushUsed)
                        text += "[ Black Flush: damage 200% ]\n";

                    if (ctx.PreviousResult.IsRedFlush)
                        text += "[ Red Flush: damage 300% ]\n";

                    text += $"Ranking: {Rankings[0]}\nDamage: {Damage}\n";
                }

                return text;
            }
        }



        public CardResult(CardContext ctx, float damage, List<MoveData> moves, List<HandRanking> rankings, List<NumberData> numbers)
        {
            this.ctx = ctx;

            Damage = damage;
            Moves = moves;
            Rankings = rankings;

            IsRedFlush = IsBlueFlush = IsGreenFlush = IsBlackFlush = false;

            if (rankings.Contains(HandRanking.Flush))
                switch (numbers[0].color)
                {
                    case NumberData.CardColor.Red: IsRedFlush = true; break;
                    case NumberData.CardColor.Blue: IsBlueFlush = true; break;
                    case NumberData.CardColor.Green: IsGreenFlush = true; break;
                    case NumberData.CardColor.Black: IsBlackFlush = true; break;
                }
        }
    }
}