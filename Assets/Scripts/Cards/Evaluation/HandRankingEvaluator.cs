using Cardevil.Cards.Core;
using Cardevil.Cards.InStage;
using Cardevil.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Cards.Evaluation
{
    public static class HandRankingEvaluator
    {
        /// <summary>
        /// 선택된 카드 목록의 족보 평가.
        /// 단순 평가 결과만 반환.
        /// </summary>
        /// <param name="selection">평가 대상 카드 목록</param>
        /// <returns>평가된 족보 결과</returns>
        public static HandRanking EvaluateHandRanking(IEnumerable<Card> selection)
        {
            var handRanking = EvaluateHandRanking(selection, out var _);
            return handRanking;
        }
        
        /// <summary>
        /// 선택된 카드 목록의 족보 평가.
        /// 족보에 포함된 카드 목록을 함께 반환.
        /// </summary>
        /// <param name="selection">평가 대상 카드 목록</param>
        /// <param name="cardsInHandRanking">평가된 족보에 속한 카드 목록</param>
        /// <returns>평가된 족보 결과</returns>
        public static HandRanking EvaluateHandRanking(IEnumerable<Card> selection, out List<Card> cardsInHandRanking)
        {
            cardsInHandRanking = null;
            
            var attackCards = selection.Where(c => c.Data.IsAttack)
                .ToList();
        
            if (attackCards.Count == 0)
                return HandRanking.None;

            if (IsStraightFlush(attackCards, out cardsInHandRanking)) 
                return HandRanking.StraightFlush;
        
            if (IsFourCard(attackCards, out cardsInHandRanking))
                return HandRanking.FourCard;
        
            if (IsStraight(attackCards, out cardsInHandRanking))
                return HandRanking.Straight;

            if (IsFlush(attackCards, out cardsInHandRanking))
            {
                var handRanking = attackCards[0].Data.Color switch
                {
                    CardColor.Red => HandRanking.RedFlush,
                    CardColor.Green => HandRanking.GreenFlush,
                    CardColor.Blue => HandRanking.BlueFlush,
                    CardColor.Black => HandRanking.BlackFlush,
                    _ => HandRanking.None
                };
            
                if (handRanking == HandRanking.None)
                    LogEx.LogError("Flush 분류에 실패했습니다.");

                return handRanking;
            }

            if (IsTriple(attackCards, out cardsInHandRanking))
                return HandRanking.Triple;
        
            if (IsTwoPair(attackCards, out cardsInHandRanking))
                return HandRanking.TwoPair;
        
            if (IsOnePair(attackCards, out cardsInHandRanking))
                return HandRanking.OnePair;

            cardsInHandRanking = new();
            return HandRanking.High;
        }
        
        private static bool IsStraight(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;

            var values = numberCards.Select(c => c.Data.NumberSelectState.FinalValue)
                    .OrderBy(v => v)
                    .ToList();

            for (int i = 1; i < numberCards.Count; i++)
                if (values[i] != values[i - 1] + 1)
                    return false;
        
            cardsInRanking = numberCards.ToList();
            return true;
        }

        private static bool IsFlush(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;
            
            bool allSameColor = true;
            for (int i = 1; i < numberCards.Count; i++)
            {
                if (numberCards[i].Data.Color == numberCards[i - 1].Data.Color)
                    continue;

                allSameColor = false;
                break;
            }

            if (allSameColor) cardsInRanking = numberCards;
            return allSameColor;
        }

        private static bool IsStraightFlush(List<Card> numberCards,  out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4) 
                return false;

            var value = IsStraight(numberCards, out var _) && IsFlush(numberCards, out var _);

            if (value) cardsInRanking = numberCards.ToList();
            return value;
        }

        private static bool IsFourCard(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();

            if (numberCards.Count < 4)
                return false;

            // 같은 숫자 값으로 그룹화
            var group = numberCards
                .GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .FirstOrDefault(g => g.Count() == 4);

            if (group != null)
            {
                cardsInRanking = group.ToList();
                return true;
            }

            return false;
        }

        private static bool IsTriple(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();

            if (numberCards.Count < 3)
                return false;

            // 같은 숫자 값으로 그룹화
            var group = numberCards
                .GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .FirstOrDefault(g => g.Count() == 3);

            if (group != null)
            {
                cardsInRanking = group.ToList();
                return true;
            }

            return false;
        }

        private static bool IsTwoPair(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            
            if (numberCards.Count != 4)
                return false;

            var groupCount = numberCards.GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .Count(g => g.Count() == 2);

            if (groupCount == 2)
            {
                cardsInRanking = numberCards.ToList();
                return true;
            }

            return false;
        }

        private static bool IsOnePair(List<Card> numberCards, out List<Card> cardsInRanking)
        {
            cardsInRanking = new List<Card>();
            if (numberCards.Count < 2)
                return false;

            var groupCount = numberCards.GroupBy(c => c.Data.NumberSelectState.FinalValue)
                .Count(g => g.Count() == 2);

            if (groupCount == 1)
            {
                cardsInRanking = numberCards.ToList();
                return true;
            }

            return false;
        }
    } 
}