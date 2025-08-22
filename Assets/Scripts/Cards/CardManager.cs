using System;
using System.Collections.Generic;
using Cardevil.Cards.CardInteractinos;
using Cardevil.Events;
using UnityEngine;

namespace Cardevil.Cards
{
    public class CardManager
    {
        [Header("Card Datas")]
        private BaseDeckConfiguration baseDeckConfig;
        private BaseDeckConfiguration baseRuntimeDeckConfig;
        public InGameDeck Deck { get; private set; }

        [Header("References")]
        private CardBarGroup barGroup;

        [Header("State")]
        public bool CanUseCard { get; private set; }

        // [Header("Events")]
        public event Action<CardResult> OnCardUsed;

        public void Init()
        {
            Managers.Turn.OnGameStateChanged += UpdateCanUseCard;

            barGroup = GameObject.Find("CardBarGroup").GetComponent<CardBarGroup>();
            if (barGroup == null) Debug.LogError("BarGroup이 씬 내 존재하지 않습니다.");
            baseDeckConfig = barGroup.baseDeckConfig;
            baseRuntimeDeckConfig = barGroup.baseRuntimeDeckConfig;

            DeckFactory.InitRuntimeDeckConfig(baseDeckConfig, baseRuntimeDeckConfig);
            Deck = new(baseRuntimeDeckConfig);
            barGroup.Init(onSelectedCardsCountChanged: UpdateCanUseCard);

            UpdateDeckCardCount();
        }

        #region Using Card
        public void UpdateDeckCardCount()
        {
            using (var args = RemainingCardChangeArgs.Get())
            {
                args.Init(Deck.Count);
                Managers.Event.RemainingCardChangeEvent?.Invoke(args);
            }
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

