using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Utils
{
    public static class CardComboEvaluator
    {
        public static CardResult Evaluate(IEnumerable<Card> cardDatas)
        {       
            var list = cardDatas
                .Select(card => card.cardData)
                .ToList();
            var result = new CardResult();

            // TODO: list 바탕으로 계산 로직 추가

            #region 임시

            // BaseScore 계산
            result.BaseDamage = 10;
            // 콤보 판정 로직
            result.Combo = CardCombo.High;
            // ComboScroe 할당
            result.ComboDamage = 9;
            
            result.directions = new CardDirection[3];
            result.directions.Append(CardDirection.Right);

            #endregion

            result.TotalDamage = result.BaseDamage + result.ComboDamage;

            return result;
        }
    }
}