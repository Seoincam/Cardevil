using Cardevil.Systems;
using System.Collections.Generic;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;

namespace Cardevil.Cards.System
{
    /// <summary>
    /// 카드 시스템 전체를 관리하는 매니저 클래스.
    /// 카드 모델, 프레젠터, 평가 이벤트 등을 초기화,
    /// 스테이지 시작 시 덱을 구성하는 역할.
    /// </summary>
    public class CardManager : IClearable
    {
        private readonly StageCardsModel _stageCardsModel = new();
        private readonly RerollPresenter _rerollPresenter = new();
        private readonly StageCardsPresenter _stageCardsPresenter = new();

        private readonly EvaluationResultsModel _evaluationResultsModel = new();
        private readonly EvaluationArgsBuilder _evaluationArgsBuilder = new();
        
        private List<CardData> _runtimeBaseDeck;
        
        public IReadOnlyEvaluationResultsModel EvaluationResults => _evaluationResultsModel;
        public IReadOnlyList<CardData> RuntimeBaseDeck => _runtimeBaseDeck;
        
        /// <summary>
        /// 카드 단계(리롤, 손패 선택 등)를 관리하는 Flow을 생성.
        /// TurnManager에서 사용.
        /// </summary>
        /// <returns><see cref="ITurnCardFlow"/> 인터페이스를 구현한 컨트롤러 인스턴스</returns>
        public ITurnCardFlow BuildFlow()
        {
            return new CardFlowController(_stageCardsModel, _rerollPresenter, _stageCardsPresenter,
                _evaluationResultsModel, _evaluationArgsBuilder);
        }
        
        /// <summary>
        /// 카드 매니저를 초기화.  
        /// 내부 상태를 초기화, 기본 덱 데이터를 생성.
        /// </summary>
        public void Init()
        {
            Clear();
            _runtimeBaseDeck = CardDataFactory.CreateBaseData();
        }

        public void Clear()
        {
            _stageCardsModel.Clear();
            _rerollPresenter.Clear();
            _stageCardsPresenter.Clear();
            
            _evaluationResultsModel.Clear();
            _evaluationArgsBuilder.Clear();
        }

        /// <summary>
        /// 스테이지 진입 시 호출.  
        /// 모델을 현재 덱으로 초기화.
        /// </summary>
        public void OnEnterStage()
        {
            Clear();
            _stageCardsModel.SetUp(InStageCardDataFactory.BuildInStageCardData(_runtimeBaseDeck), 6,3);
            
            // TODO: 나중에 어떤식으로 할지 기획 나오면 제대로 분리해야함
            // var deckRemains =
            //     Object.FindObjectsByType<DeckRemain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            // if (deckRemains == null || deckRemains.Length == 0) { LogEx.LogError("Deck Remain이 씬내에 존재하지 않음!"); return; }
            // var deckRemain = deckRemains[0];
            // deckRemain.Init(_stageCards);
        }
       
        public ILockable GetCard()
        {
            // return StageCardsCtx.GetRandomCard();
            return null;
        }

        /// <summary>
        /// 현재 선택된 족보에 따른 점수를 계산.
        /// 족보 데이터베이스를 참조하여 점수를 반환.
        /// </summary>
        /// <returns>현재 족보에 해당하는 점수</returns>
        // public int GetCurrentCardRankScore()
        // {
        //     var result = ResultCtx.CurrentResult;
        //     if (result == null)
        //     {
        //         Debug.LogError("잘못된 시점에 족보에 접근.");
        //         result = ResultCtx.PreviousResult;
        //     }
        //     HandRanking rank = result.Ranking;
        //
        //     var data = Managers.Database.Database.HandRankingDataList
        //         .FirstOrDefault(r => r.Ranking == rank);
        //
        //     int score = data?.Value ?? 0;
        //     return score;
        // }
    }
}


