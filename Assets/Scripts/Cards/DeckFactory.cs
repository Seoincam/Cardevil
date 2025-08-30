using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public static class DeckFactory
    {
        /// <summary>
        /// 런타임에 진입할 때, 게임 전체에서 공유될 Deck 생성
        /// </summary>
        /// <param name="originDeck">기본 카드 구성</param>
        /// <param name="runtimeDeck">기본 카드 구성으로부터 복사해 런타임에 사용할 카드 구성</param>
        public static void CreateRuntimeDeck(BaseDeckConfiguration originDeck, BaseDeckConfiguration runtimeDeck)
        {
            originDeck.InitBaseDeckConfig();
            runtimeDeck.Deck.Clear();

            foreach (var cardData in originDeck.Deck)
                runtimeDeck.Deck.Add(cardData.Copy());
        }

        /// <summary>
        /// 스테이지에 진입할 때, 스테이지에서만 사용될 Deck 생성 및 셔플
        /// </summary>
        public static List<CardData> CreateStageDeck(BaseDeckConfiguration runtimeDeck)
        {
            var deck = new List<CardData>(50);
            foreach (var cardData in runtimeDeck.Deck)
                deck.Add(cardData.Copy());

            for (int i = 0; i < deck.Count; i++)
            {
                var randomIndex = Random.Range(0, deck.Count);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
            return deck;
        }
    }
}

