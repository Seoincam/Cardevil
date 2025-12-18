using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Cards.Data;
using Cardevil.Cards.Data.Enhancement;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Cards.OutStage;
using Cardevil.Core.Turn.Interfaces;
using Cysharp.Threading.Tasks;
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
        private readonly CardsModel _cardsModel = new();
        private readonly RerollPresenter _rerollPresenter = new();
        private readonly StageCardsPresenter _stageCardsPresenter = new();

        private readonly EvaluationResultsModel _evaluationResultsModel = new();
        private readonly EvaluationPresenter _evaluationPresenter = new();
        
        #region IReadOnly

        public IReadOnlyEvaluationResultsModel EvaluationResults => _evaluationResultsModel;

        #endregion
        
        /// <summary>
        /// 카드 단계(리롤, 손패 선택 등)를 관리하는 Flow을 생성.
        /// TurnManager에서 사용.
        /// </summary>
        /// <returns><see cref="ITurnCardFlow"/> 인터페이스를 구현한 컨트롤러 인스턴스</returns>
        public ITurnCardFlow BuildFlow()
            => new CardFlowController(_status, _cardsModel, _rerollPresenter, _stageCardsPresenter, _evaluationPresenter);

        private IReadOnlyCardStatus _status; // TODO: 얘는 없어야함.

        /// <summary>
        /// 카드 매니저를 초기화.  
        /// 내부 상태를 초기화.
        /// </summary>
        public async UniTask InitAsync(IReadOnlyCardStatus status)
        {
            Clear();
            _status = status;
            _evaluationPresenter.Init(_evaluationResultsModel);
        }

        public void Clear()
        {
            _cardsModel.Clear();
            _rerollPresenter.Clear();
            _stageCardsPresenter.Clear();
            
            _evaluationResultsModel.Clear();
        }
       
        public ILockable GetCard()
        {
            // return StageCardsCtx.GetRandomCard();
            return null;
        }
    }
}


