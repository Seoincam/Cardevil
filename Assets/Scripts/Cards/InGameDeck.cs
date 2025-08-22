using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cardevil.Cards
{
    /// <summary>
    /// 각 게임마다 초기화되는 덱 
    /// </summary>
    public class InGameDeck
    {
        private readonly List<CardData> deck;
        private readonly List<CardData> discardPile;
        // TODO: 버려진 카드 관련 로직 추가

        public int Count => deck.Count();

        public InGameDeck(BaseDeckConfiguration baseDeck)
        {
            deck = DeckFactory.InitInGameDeck(baseDeck);
            discardPile = new();
        }

        public CardData DrawCard()
        {
            if (deck.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = deck.First();
            deck.RemoveAt(0);
            // UpdateDeckCardCount();

            return cardData;
        }

    }
}
