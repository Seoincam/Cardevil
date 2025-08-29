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
        // TODO: 버려진 카드를 저장할 필요가 있을 지 확인.
        private readonly List<CardData> discard;

        public int Count => deck.Count();

        // 오로지 덱의 상태만 전달 -> 플레이어 턴 여부 등은 고려x
        public bool CanUseCard;

        /// <summary>
        /// baseDeck은 게임 중 공유되는 덱: 강화 등을 저장함.
        /// / baseDeck을 바탕으로 InGameDeck에서 덱을 생성: 그 게임에서만 사용됨.
        /// </summary>
        public InGameDeck(BaseDeckConfiguration baseDeck)
        {
            deck = DeckFactory.InitInGameDeck(baseDeck);
            discard = new();
        }

        public CardData GetRandomCard()
        {
            int randomIndex = Random.Range(0, deck.Count());
            return deck[randomIndex];
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
