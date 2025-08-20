using System;
using System.Collections.Generic;
using System.Linq;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Events;
using Cardevil.Utils.Directions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Cardevil.Cards
{
    public class CardManager
    {
        [Header("Card Datas")]
        public List<CardData> defaultCards;
        public List<CardData> deckCards;

        [Header("References")]
        private CardBarGroup barGroup;

        [Header("State")]
        public bool CanUseCard { get; private set; }

        // [Header("Events")]
        public event Action<CardResult> OnCardUsed;

        public void Init()
        {
            Managers.Turn.OnGameStateChanged += UpdateCanUseCard;
            InitCards();
            InitDeck();
            UpdateDeckCardCount();

            barGroup = GameObject.Find("CardBarGroup").GetComponent<CardBarGroup>();
            if (barGroup == null)
                Debug.LogError("BarGroup이 씬 내 존재하지 않습니다.");
            barGroup.Init(onSelectedCardsCountChanged: UpdateCanUseCard);
        }

        // 카드를 기본 상태로 초기화
        private void InitCards()
        {
            defaultCards = new(50);

            foreach (var color in Enum.GetValues(typeof(CardColor)).Cast<CardColor>())
            {
                for (int value = 2; value <= 10; value++)
                {
                    var card = new NumberCard(color, value);
                    defaultCards.Add(card);
                }

                var cardData = new NumberCard(color, defaultValue: 0, canSelect: true);
                defaultCards.Add(cardData);
            }

            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                if (direction == Direction.None)
                    continue;

                for (int i = 0; i < 2; i++)
                {
                    var card = new DirectionCard(direction, canSelect: false);
                    defaultCards.Add(card);
                }
            }

            for (int i = 0; i < 2; i++)
            {
                var card = new DirectionCard(Direction.None, canSelect: true);
                defaultCards.Add(card);
            }

            Debug.Log(defaultCards.Count);
        }

        // defaultCards를 바탕으로 덱을 초기화
        private void InitDeck()
        {
            deckCards = defaultCards.ToList();
            for (int i = 0; i < deckCards.Count; i++)
            {
                var randomIndex = Random.Range(0, deckCards.Count);
                (deckCards[i], deckCards[randomIndex]) = (deckCards[randomIndex], deckCards[i]);
            }
        }

        #region Using Card

        private void UpdateDeckCardCount()
        {
            using (var args = RemainingCardChangeArgs.Get())
            {
                args.Init(deckCards.Count);
                Managers.Event.RemainingCardChangeEvent?.Invoke(args);
            }
        }

        public CardData DrawCard()
        {
            if (deckCards.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = deckCards.First();
            deckCards.RemoveAt(0);
            UpdateDeckCardCount();

            return cardData;
        }

        public void UseCard(IEnumerable<Card> cards)
        {
            var cardResult = CardComboEvaluator.Evaluate(cards);

            OnCardUsed?.Invoke(cardResult);
            // 임시로
            Managers.Game.Player.Move(cardResult.moves);
        }

        private void UpdateCanUseCard()
        {
            CanUseCard = Managers.Game.currentState == GameManager.GameState.PlayerInput
                ? barGroup.selectedCards.Count > 0
                : false;

            barGroup.SetUseCardButton(interactable: CanUseCard);
        }
        
        #endregion
    }
}

