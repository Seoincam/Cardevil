using Cardevil.Systems;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using System.Linq;

namespace Cardevil.Cards
{
    public class CardManager: IClearable
    {
        public ICardHandBar handBar;
        public ITurnPlayerInput playerInput;

        private CardResultContext _resultCtx = new();
        private AsyncEvaluationEvent _evaluations = new();
        private List<CardData> _runtimeBaseDeck;
        private int _maxHandCount = 6;



        public CardResultContext ResultCtx => _resultCtx;

        public AsyncEvaluationEvent Evaluations => _evaluations;

        public IReadOnlyList<CardData> RuntimeBaseDeck => _runtimeBaseDeck;

        public int MaxHandCount => _maxHandCount;


        public void Init()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            handBar = handBarObj.GetComponent<ICardHandBar>();
            handBar.Init();
            playerInput = handBarObj.GetComponent<ITurnPlayerInput>();
            if (playerInput == null) Debug.LogError("playerinput이 없습니다.");

            _runtimeBaseDeck = DeckFactory.CreateRuntimeBaseDeck();
        }

        public void OnEnterStage()
        {
            handBar.Clear();
        }

        public void Clear()
        {
            _resultCtx.Clear();
        }


        public ILockable GetCard()
        {
            return handBar.StageCardsCtx.GetRandomCard();
        }

        public int GetCurrentCardRankScore()
        {
            var result = _resultCtx.CurrentResult;
            if (result == null)
            {
                Debug.LogError("잘못된 시점에 족보에 접근.");
                result = _resultCtx.PreviousResult;
            }
            HandRanking rank = result.Ranking;

            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(r => r.Ranking == rank);

            int score = data?.Value ?? 0;
            return score;
        }
    }
}


