using Cardevil.Cards.Evaluation;
using Cardevil.Cards.InStage;
using Cysharp.Threading.Tasks;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn.Interfaces;
using System.Threading;
using UnityEngine;

namespace Cardevil.Cards.Core
{
    public sealed class CardFlowController : ITurnCardFlow
    {
        private StageCardsModel _cardsModel;
        private RerollPresenter _rerollPresenter;
        private StageCardsPresenter _stageCardsPresenter;
        private IEvaluationPresenter _evaluationPresenter;
        
        public static CardFlowController Build()
        {
            var status = CardStatus.Current;
            var initialDeck = new CardData[status.Count];
            
            for (int i = 0; i < status.Count; i++)
            {
                initialDeck[i] = status.GetCardDataById(i);
                Debug.Assert(initialDeck[i] != null, $"[CardStatus] data[{i}] is null.");
            }

            var maxHand = CardevilCore.Instance.Game.PlayerStatus.MaxHand;
            var discardRemain = CardevilCore.Instance.Game.PlayerStatus.DiscardCard;
            var model = new StageCardsModel(initialDeck, maxHand, discardRemain);

            var rerollPresenter = new RerollPresenter(model);
            var stageCardsPresenter = new StageCardsPresenter(model);
            EvaluationPresenter evaluationPresenter = null;

            return new CardFlowController
            {
                _cardsModel = model,
                _rerollPresenter = rerollPresenter,
                _stageCardsPresenter = stageCardsPresenter,
                _evaluationPresenter = evaluationPresenter
            };
        }

        public bool IsNoCard => _cardsModel.Hand.Count == 0;
        
        public async UniTask DrawCard()
        {
            await _stageCardsPresenter.DrawUntilMaxHandAsync();
        }

        public async UniTask WaitUserInput()
        {
            await _stageCardsPresenter.WaitPlayerInputCompleted();
        }
        
        public async UniTask UseAllCardsAsync(CancellationToken cancellationToken) =>
            await _stageCardsPresenter.UseAllCardsAsync();

        public async UniTask<EvaluationResult> EvaluateAsync(CancellationToken cancellationToken)
        {
            var evaluationResult = await _evaluationPresenter.EvaluateAsync(cancellationToken);
            return evaluationResult;
        }
            
        public EvaluationResult? Result => _evaluationPresenter.GetCurrentEvaluationResult();
        
        public IReadOnlyEvaluationResultsModel ResultsModel => _evaluationPresenter.ResultsModel;

        public IReadOnlyStageCardsModel CardsModel => _cardsModel;
    }
}