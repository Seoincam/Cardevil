using System;
using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public class CardManager : MonoBehaviour
    {
        [Header("Card Datas")]
        public List<CardData> cardDatas;

        [Header("Reference")]
        public CardBarGroup barGroup;

        void Start()
        {
            Init();
            barGroup.Init(cardManager: this);
        }

        public void Init()
        {
            // Card data 생성
            cardDatas = new(50);

            foreach (var color in Enum.GetValues(typeof(CardColor)).Cast<CardColor>())
            {
                if (color == CardColor.None)
                    continue;

                for (int i = 1; i <= 10; i++)
                {
                    var cardData = new CardData(color, value: i);
                    cardDatas.Add(cardData);
                }
            }

            foreach (var direction in Enum.GetValues(typeof(CardDirection)).Cast<CardDirection>())
            {
                if (direction == CardDirection.None)
                    continue;

                for (int i = 0; i < 2; i++)
                {
                    var cardData = new CardData(direction);
                    cardDatas.Add(cardData);
                }
            }

            // Card Data 셔플
            for (int i = 0; i < cardDatas.Count; i++)
            {
                var randomIndex = Random.Range(0, cardDatas.Count);
                (cardDatas[i], cardDatas[randomIndex]) = (cardDatas[randomIndex], cardDatas[i]);
            }
        }
    }
}

