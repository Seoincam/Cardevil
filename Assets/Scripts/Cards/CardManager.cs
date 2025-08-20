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
        public List<CardData> cardDatas;

        [Header("References")]
        private CardBarGroup barGroup;

        [Header("State")]
        public bool CanUseCard { get; private set; }

        // [Header("Events")]
        public event Action<CardResult> OnCardUsed;

        public void Init()
        {
            Managers.Turn.OnGameStateChanged += UpdateCanUseCard;

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

            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                if (direction == Direction.None)
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

            UpdateDeckCardCount();

            barGroup = GameObject.Find("CardBarGroup").GetComponent<CardBarGroup>();
            if (barGroup == null)
                Debug.LogError("BarGroup이 씬 내 존재하지 않습니다.");
            barGroup.Init(onSelectedCardsCountChanged: UpdateCanUseCard);
        }

        #region Using Card

        private void UpdateDeckCardCount()
        {
            using (var args = RemainingCardChangeArgs.Get())
            {
                args.Init(cardDatas.Count);
                Managers.Event.RemainingCardChangeEvent?.Invoke(args);  
            }
        }

        public CardData? DrawCard()
        {
            if (cardDatas.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }

            var cardData = cardDatas.First();
            cardDatas.RemoveAt(0);
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

