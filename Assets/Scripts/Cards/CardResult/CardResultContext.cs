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

    public class CardContext
    {
        public readonly MultiplyValues Multiply;
        public CardResult PreviousResult { get; private set; }
        public CardResult CurrentResult { get; private set; }

        public void GetSet()
        {
            PreviousResult = CurrentResult;
        }

        public void SetResult(CardResult result)
        {
            CurrentResult = result;
        }

        public CardContext(MultiplyValues multiplyValues)
        {
            Multiply = multiplyValues;
        }
    }

    /// <summary>
    /// 사용한 카드를 바탕으로 해석된 결과만을 가지는 구조체
    /// </summary>
    public readonly struct CardResult
    {
        public readonly float Damage;
        public readonly List<MoveData> Moves;

        public readonly List<HandRanking> Rankings;
        public readonly bool IsRedCardOver3;
        public readonly bool IsBlackCardOver3;
        
        public string Description
        {
            get => Rankings.Count > 0 ? $"{Rankings[0]}, Damage: {Damage}" : "";
        }

        public CardResult(float damage, List<MoveData> moves, List<HandRanking> rankings, bool isRedCardOver3, bool isBlackCardOver3)
        {
            Damage = damage;
            Moves = moves != null ? new(moves) : new();

            Rankings = rankings != null ? new(rankings) : new();
            IsRedCardOver3 = isRedCardOver3;
            IsBlackCardOver3 = isBlackCardOver3;
        }
    }
}