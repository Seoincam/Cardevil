using Cardevil.Cards.Data;
using Cardevil.Cards.Evaluations;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core.Turn.Interfaces;
using System.Threading;

namespace Cardevil.Cards.System
{
    public sealed class CardFlowController : ITurnCardFlow
    {
        private readonly IReadOnlyCardStatus _status;
        
        private readonly CardsModel _cardsModel;
        private readonly RerollPresenter _rerollPresenter;
        private readonly StageCardsPresenter _stageCardsPresenter;
        private readonly IEvaluationPresenter _evaluationPresenter;

        private int _maxHand;
        
        public CardFlowController(IReadOnlyCardStatus status,
            CardsModel cardsModel, RerollPresenter rerollPresenter,
            StageCardsPresenter stageCardsPresenter, IEvaluationPresenter evaluationPresenter)
        {
            _status = status;
            
            _cardsModel = cardsModel;
            _rerollPresenter = rerollPresenter;
            _stageCardsPresenter = stageCardsPresenter;
            
            _evaluationPresenter = evaluationPresenter;
            
            _rerollPresenter.Init(_status, _cardsModel);
            _stageCardsPresenter.Init(_status, _cardsModel, _evaluationPresenter);
        }

        // TODO: 구현해야함.
        public bool IsNoCard => _cardsModel.Hand.Count == 0;
        
        public async UniTask DrawCard()
        {
            await _stageCardsPresenter.DrawCard();
        }

        public async UniTask WaitUserInput()
        {
            await _stageCardsPresenter.WaitUserInput();
        }
        
        public async UniTask UseAllCardsAsync(CancellationToken cancellationToken) =>
            await _stageCardsPresenter.UseAllCardsAsync(cancellationToken);

        public async UniTask<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken)
        {
            var evaluationResult = await _evaluationPresenter.EvaluateAsync(cancellationToken);
            return evaluationResult;
        }
            
        public EvaluationResult? Result => _evaluationPresenter.GetCurrentEvaluationResult();
        
        public IReadOnlyEvaluationResultsModel ResultsModel => _evaluationPresenter.ResultsModel;

        public IReadOnlyCardsModel CardsModel => _cardsModel;
    }
}