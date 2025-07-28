using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards;

namespace Cardevil.Utils
{
    public static class CardComboEvaluator
    {
        public static CardResult Evaluate(IEnumerable<CardData> cards)
        {
            var list = cards.ToList();
            var result = new CardResult();

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