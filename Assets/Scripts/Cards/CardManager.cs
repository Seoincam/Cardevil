using Cardevil.Systems;
using UnityEngine;

namespace Cardevil.Cards
{
    public class CardManager
    {
        public ICardHandBar handBar;
        public ITurnPlayerInput playerInput;

        public void Init()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            handBar = handBarObj.GetComponent<ICardHandBar>();
            playerInput = handBarObj.GetComponent<ITurnPlayerInput>();
            if (playerInput == null) Debug.LogError("playerinput이 없습니다.");
 
            handBar.Init();
        }

        public ILockable GetCard()
        {
            return handBar.Deck.GetRandomCard();
        }

        public CardResult GetCurrentCardRank()
        {

            return Managers.Card.handBar.Context.CurrentResult;
        }

        public int GetCurrentCardRankScore()
        {

            CardResult card = GetCurrentCardRank();

            HandRanking rank = card.Rankings[0];

            // enum → int 변환
            int score = (int)rank;

            return score;
        }
        
    }
}

