using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public static class DeckFactory
    {
        /// <summary>
        /// 런타임에 진입할 때, 새 DeckConfig 생성
        /// </summary>
        /// <param name="origin">기본 카드 구성</param>
        /// <param name="runtime">기본 카드 구성으로부터 복사해 런타임에 사용할 카드 구성</param>
        public static void InitRuntimeDeckConfig(BaseDeckConfiguration origin, BaseDeckConfiguration runtime)
        {
            runtime.Clear();

            runtime.numberCardDatas = new(40);
            foreach (var num in origin.numberCardDatas)
                runtime.numberCardDatas.Add((NumberCardData)num.CreateInGame());

            runtime.directionCardDatas = new(10);
            foreach (var dir in origin.directionCardDatas)
                runtime.directionCardDatas.Add((DirectionCardData)dir.CreateInGame());
        }

        /// <summary>
        /// 게임을 시작할 때, Deck 초기화
        /// </summary>
        public static List<CardData> InitInGameDeck(BaseDeckConfiguration baseDeck)
        {
            var deck = new List<CardData>();

            foreach (var num in baseDeck.numberCardDatas)
                deck.Add(num.CreateInGame());
            foreach (var dir in baseDeck.directionCardDatas)
                deck.Add(dir.CreateInGame());

            for (int i = 0; i < deck.Count; i++)
            {
                var randomIndex = Random.Range(0, deck.Count);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }

            return deck;
        }
    }
}

