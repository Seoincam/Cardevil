using Cardevil.Utils.Directions;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Cardevil.Cards
{
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
        public readonly bool isSet;

        public readonly float Damage;
        public readonly List<Direction> Moves;

        public readonly List<HandRanking> Rankings;
        public readonly bool IsRedCardOver3;
        public readonly bool IsBlackCardOver3;

        public CardResult(float damage, List<Direction> moves, List<HandRanking> rankings, bool isRedCardOver3, bool isBlackCardOver3)
        {
            isSet = true;

            Damage = damage;
            Moves = new(moves);
            
            Rankings = new(rankings);
            IsRedCardOver3 = isRedCardOver3;
            IsBlackCardOver3 = isBlackCardOver3;
        }
    }
}