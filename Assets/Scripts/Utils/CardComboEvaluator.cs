using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards;

namespace Cardevil.Utils
{
    public static class CardComboEvaluator
    {
        public enum CardCombo
        {
            High,
            OnePair,
            TwoPair,
            Triple,
            Straight,
            Flush,
            StraightFlush,  // 스티플
            FourCard
        }

        public struct Result
        {
            public CardCombo Combo;
            public int BaseScore;    // 카드들의 합계
            public int ComboScore;    // 콤보의 추가 점수
            public int TotalScore;
        }

        public static Result Evaluate(IEnumerable<CardData> cards)
        {
            var list = cards.ToList();
            var result = new Result();

            // BaseScore 계산
            result.BaseScore = 10;  // 임시
            // 콤보 판정 로직
            result.Combo = CardCombo.High;   // 임시
            // ComboScroe 할당
            result.ComboScore = 9; // 임시
            
            result.TotalScore = result.BaseScore + result.ComboScore;

            return result;
        }

    }
}