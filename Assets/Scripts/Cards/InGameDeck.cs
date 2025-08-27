using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cardevil.Cards.CardInteractinos;

namespace Cardevil.Cards
{
    /// <summary>
    /// 각 게임마다 초기화되는 덱 
    /// </summary>
    public class InGameDeck
    {
        private readonly List<CardData> deck;
        private readonly List<CardData> discard;

        public int Count => deck.Count();

        // 오로지 덱의 상태만 전달 -> 플레이어 턴 여부 등은 고려x
        public bool CanUseCard;

        public InGameDeck(BaseDeckConfiguration baseDeck)
        {
            deck = DeckFactory.InitInGameDeck(baseDeck);
            discard = new();
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
