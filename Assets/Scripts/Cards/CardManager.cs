using System;
using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Systems;
using Cardevil.Utils;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public class CardManager : MonoBehaviour
    {
        [Header("Card Datas")]
        public List<CardData> cardDatas;

        [Header("References")]
        public CardBarGroup barGroup;
        [SerializeField] Button useCardButton;

        [Header("State")]
        private bool canUseCard;

        // [Header("Events")]
        public event Action<CardResult> OnUseCard;


        void Start()
        {
            Init();
            TurnManager.Instance.OnGameStateChanged += UpdateCanUseCard;

            barGroup.Init(cardManager: this, onSelectedCardsCountChanged: UpdateCanUseCard);
            useCardButton.onClick.AddListener(UseCard);
        }

        public void Init()
        {
            // Card data 생성
            cardDatas = new(50);

            foreach (var color in Enum.GetValues(typeof(CardColor)).Cast<CardColor>())
            {
                if (color == CardColor.None)
                    continue;

                for (int i = 2; i <= 11; i++)
                {
                    var cardData = new CardData(color, value: i, reinforce: 0);
                    cardDatas.Add(cardData);
                }
            }

            foreach (var direction in Enum.GetValues(typeof(CardDirection)).Cast<CardDirection>())
            {
                if (direction == CardDirection.None)
                    continue;

                for (int i = 0; i < 2; i++)
                {
                    var cardData = new CardData(direction, reinforce: 0);
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

        #region Using Card

        private void UpdateCanUseCard()
        {
            canUseCard = TurnManager.Instance.gameState == GameState.PlayerInput
                ? barGroup.selectedCards.Count > 0
                : false;

            useCardButton.interactable = canUseCard;
        }

        private void UseCard()
        {
            if (!canUseCard)
                return;

            var cardResult = CardComboEvaluator.Evaluate(barGroup.selectedCards);

            OnUseCard?.Invoke(cardResult);

            _ = barGroup.DiscardSequentially();
        }
        
        #endregion
    }
}

