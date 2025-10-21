using Cardevil.Cards.Evaluations;
using Cardevil.Systems;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Presenter;

namespace Cardevil.Cards.System
{
    public sealed class CardFlowController : ITurnCardFlow
    {
        private readonly StageCardsModel _stageCardsModel;
        private readonly RerollPresenter _rerollPresenter;
        private readonly StageCardsPresenter _stageCardsPresenter;

        private readonly EvaluationResultsModel _evaluationResultsModel;
        private readonly EvaluationArgsBuilder _evaluationArgsBuilder;

        private int _maxHand;

        public ITurnRerollInput Reroll => _rerollPresenter;
        public ITurnPlayerInput StageCards => _stageCardsPresenter;

        public CardFlowController(StageCardsModel stageCardsModel, RerollPresenter rerollPresenter,
            StageCardsPresenter stageCardsPresenter, EvaluationResultsModel evaluationResultsModel, 
            EvaluationArgsBuilder evaluationArgsBuilder)
        {
            _stageCardsModel = stageCardsModel;
            _rerollPresenter = rerollPresenter;
            _stageCardsPresenter = stageCardsPresenter;
            
            _evaluationResultsModel = evaluationResultsModel;
            _evaluationArgsBuilder = evaluationArgsBuilder;
        }

        public async UniTask EnterRerollPhase(int maxHand)
        {
            _maxHand = maxHand;
            
            _rerollPresenter.Init(_stageCardsModel);
            await _rerollPresenter.SetUp(maxHand);
        }

        public async UniTask ExitRerollPhase()
        {
            await _rerollPresenter.Exit();
        }

        public void DeactivateReroll()
        {
            _rerollPresenter.Clear();
        }

        public async UniTask EnterHandPhase()
        {
            _evaluationArgsBuilder.Init(_evaluationResultsModel);
            _stageCardsPresenter.Init(_stageCardsModel, _evaluationArgsBuilder);
            await _stageCardsPresenter.SetUp(_maxHand);
            DeactivateReroll();
        }

        public async UniTask ExitHandPhase()
        {
            await _stageCardsPresenter.Exit();
        }

        public void DeactivateHandPhase()
        {
            _stageCardsPresenter.Clear();
        }
    }
}