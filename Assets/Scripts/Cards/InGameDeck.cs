using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cardevil.Cards
{
    /// <summary>
    /// 매 스테이지마다 초기화되는 덱 
    /// </summary>
    public class InStageDeck
    {
        private List<CardData> Deck;
        // TODO: 버려진 카드를 저장할 필요가 있을 지 확인.
        private List<CardData> Discards;

        public int Count => Deck.Count();

        // 오로지 덱의 상태만 전달 -> 플레이어 턴 여부 등은 고려x
        public bool CanUseCard;

        public void Init(List<CardData> deck)
        {
            Deck = deck;
            Discards = new();
        }

        public CardData GetRandomCard()
        {
            int randomIndex = Random.Range(0, Deck.Count());
            return Deck[randomIndex];
        }

        public CardData DrawCard()
        {
            if (Deck.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = Deck.First();
            Deck.RemoveAt(0);
            // UpdateDeckCardCount();

            return cardData;
        }

    }
}
