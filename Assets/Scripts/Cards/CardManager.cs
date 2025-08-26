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
        private CardHandBar handBar;

        public void Init()
        {
            // Managers.Turn.OnGameStateChanged += UpdateCanUseCard;
            handBar = GameObject.Find("CardHandBar").GetComponent<CardHandBar>();
            if (handBar == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            baseDeckConfig = handBar.baseDeckConfig;
            baseRuntimeDeckConfig = handBar.baseRuntimeDeckConfig;

            DeckFactory.InitRuntimeDeckConfig(baseDeckConfig, baseRuntimeDeckConfig);
            Deck = new(baseRuntimeDeckConfig);
            handBar.Init();

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
        #endregion
    }
}

