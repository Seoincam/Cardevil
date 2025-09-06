using Cardevil.Systems;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards
{
    public class CardManager
    {
        public ICardHandBar handBar;
        public ITurnPlayerInput playerInput;

        public List<CardData> runtimeBaseDeck;

        public void Init()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            handBar = handBarObj.GetComponent<ICardHandBar>();
            playerInput = handBarObj.GetComponent<ITurnPlayerInput>();
            if (playerInput == null) Debug.LogError("playerinput이 없습니다.");

            runtimeBaseDeck = DeckFactory.CreateRuntimeBaseDeck();
            handBar.Init();
        }

        public ILockable GetCard()
        {
            return handBar.StageCardsCtx.GetRandomCard();
        }

        public int GetCurrentCardRankScore()
        {
            HandRanking rank = handBar.Context.CurrentResult.Rangking;

            // enum → int 변환
            int score = (int)rank;

            return score;
        }
        
    }
}

