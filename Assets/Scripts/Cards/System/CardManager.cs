using Cardevil.Systems;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.OutStage;
using System;
using UnityEngine;

namespace Cardevil.Cards.System
{
    /// <summary>
    /// 카드 시스템 전체를 관리하는 매니저 클래스.
    /// 카드 모델, 프레젠터, 평가 이벤트 등을 초기화,
    /// 스테이지 시작 시 덱을 구성하는 역할.
    /// </summary>
    [Serializable]
    public class CardManager : IClearable
    {
        [SerializeField] private CardLibrary cardLibrary = new();
        [SerializeField] private EnhancementDataLibrary enhancementDataLibrary = new();
        
        // Out Stage
        private readonly CardPipelineModifierService _modifierService = new();
        private readonly CardEnhancementPresenter _enhancementPresenter = new();
        
        // In Stage
        private readonly StageCardsModel _stageCardsModel = new();
        private readonly RerollPresenter _rerollPresenter = new();
        private readonly StageCardsPresenter _stageCardsPresenter = new();

        private readonly EvaluationResultsModel _evaluationResultsModel = new();
        private readonly EvaluationArgsBuilder _evaluationArgsBuilder = new();

        #region IReadOnly

        public IReadOnlyCardLibrary CardLibrary => cardLibrary;
        public IReadOnlyEvaluationResultsModel EvaluationResults => _evaluationResultsModel;

        #endregion
        
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
            
            cardLibrary.Init(enhancementDataLibrary);
            enhancementDataLibrary.Init();

            _modifierService.Init(cardLibrary);
            _enhancementPresenter.Init(cardLibrary, enhancementDataLibrary, _modifierService);
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
            _stageCardsModel.SetUp(CardDataFactory.BuildInStageCardData(cardLibrary.Pipelines), 6,3);
            
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
    }
}


