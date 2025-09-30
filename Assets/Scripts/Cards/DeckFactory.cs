using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public static class DeckFactory
    {
        /// <summary>
        /// 런타임에 진입할 때, 런타임 중 사용될 Base Deck을 생성
        /// </summary>
        public static List<CardData> CreateRuntimeBaseDeck()
        {
            var deck = new List<CardData>();
            CardData card;
            NumberData number;
            MoveData move;
            int id = 0;

            // 숫자 카드
            foreach (NumberData.CardColor color in Enum.GetValues(typeof(NumberData.CardColor)))
            {
                if (color == NumberData.CardColor.None)
                    continue;

                for (int num = 2; num <= 10; num++)
                {
                    number = new(color, num);
                    move = new();

                    card = new(id++, number, move, CardData.ValueType.Number);
                    deck.Add(card);
                }

                number = new(color, 0);
                move = new();
                card = new(id++, number, move, CardData.ValueType.Number, CardData.SelectType.All);
                deck.Add(card);
            }

            // 방향 카드
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (direction == Direction.None)
                    {
                        number = new();
                        move = new();
                        card = new(id++, number, move, CardData.ValueType.Move, CardData.SelectType.All);
                        deck.Add(card);
                    }
                    else
                    {
                        number = new();
                        move = new(direction, 1);
                        card = new(id++, number, move, CardData.ValueType.Move);
                        deck.Add(card);
                    }
                }
            }

            return deck;
        }

        /// <summary>
        /// 스테이지에 진입할 때, 스테이지에서만 사용될 Deck 생성 및 셔플
        /// </summary>
        public static List<CardData> CreateStageDeck(List<CardData> runtimeDeck)
        {
            var deck = new List<CardData>(50);
            foreach (var cardData in runtimeDeck)
                deck.Add(cardData.DeepClone());

            for (int i = 0; i < deck.Count; i++)
            {
                var randomIndex = Random.Range(0, deck.Count);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
            return deck;
        }
    }
}

