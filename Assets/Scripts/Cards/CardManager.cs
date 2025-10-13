using Cardevil.Systems;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using System.Linq;
using Cardevil.Cards.Interactions;

namespace Cardevil.Cards
{
    public class CardManager : IClearable
    {
        public StageCardsPresenter StageCardsPresenter { get; } = new();
        public RerollPresenter RerollPresenter { get; } = new();
        
        public CardResultContext ResultCtx { get; } = new();
        public AsyncEvaluationEvent EvaluationEvent { get; } = new();
        
        private readonly StageCardsModel _stageCardsModel = new();
        private List<CardData> _runtimeBaseDeck;

        private int _maxHandCount = 6;
            
        public IReadOnlyList<CardData> RuntimeBaseDeck
        {
            get => _runtimeBaseDeck;
        }
        
        public int MaxHandCount
        {
            get => _maxHandCount;
        }

        public ITurnCardFlow BuildFlow()
        {
            return new CardFlowController(_stageCardsModel, RerollPresenter, StageCardsPresenter);
        }

        
        public void Init()
        {
            Clear();
            _runtimeBaseDeck = DeckFactory.CreateRuntimeBaseDeck();
        }

        public void Clear()
        {
            StageCardsPresenter.Clear();
            _stageCardsModel.Clear();
            ResultCtx.Clear();
        }

        public void OnEnterStage()
        {
            Clear();
            _stageCardsModel.InitializeDeck(_runtimeBaseDeck, 3);
        }
       
        public ILockable GetCard()
        {
            // return StageCardsCtx.GetRandomCard();
            return null;
        }

        public int GetCurrentCardRankScore()
        {
            var result = ResultCtx.CurrentResult;
            if (result == null)
            {
                Debug.LogError("잘못된 시점에 족보에 접근.");
                result = ResultCtx.PreviousResult;
            }
            HandRanking rank = result.Ranking;

            var data = Managers.Database.Database.HandRankingDataList
                .FirstOrDefault(r => r.Ranking == rank);

            int score = data?.Value ?? 0;
            return score;
        }
    }
}


