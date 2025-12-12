using Cardevil.Cards.Data;
using Cardevil.Cards.Evaluations;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Presenter;
using Cardevil.Core.Turn.Interfaces;

namespace Cardevil.Cards.System
{
    public sealed class CardFlowController : ITurnCardFlow
    {
        private readonly CardLibrary _library;
        
        private readonly StageCardsModel _stageCardsModel;
        private readonly RerollPresenter _rerollPresenter;
        private readonly StageCardsPresenter _stageCardsPresenter;
        private readonly IEvaluationPresenter _evaluationPresenter;

        private int _maxHand;
        
        public CardFlowController(CardLibrary library,
            StageCardsModel stageCardsModel, RerollPresenter rerollPresenter,
            StageCardsPresenter stageCardsPresenter, IEvaluationPresenter evaluationPresenter)
        {
            _library = library;
            
            _stageCardsModel = stageCardsModel;
            _rerollPresenter = rerollPresenter;
            _stageCardsPresenter = stageCardsPresenter;
            
            _evaluationPresenter = evaluationPresenter;
        }

        public async UniTask EnterRerollPhase(int maxHand)
        {
            _maxHand = maxHand;
            
            _rerollPresenter.Init(_library, _stageCardsModel);
            await _rerollPresenter.SetUp(maxHand);
        }

        public async UniTask Reroll()
        {
            await _rerollPresenter.Reroll();
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
            _stageCardsPresenter.Init(_library, _stageCardsModel, _evaluationPresenter);
            await _stageCardsPresenter.SetUp();
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

        // TODO: 구현해야함.
        public bool IsNoCard { get; }
        
        public async UniTask DrawCard()
        {
            await _stageCardsPresenter.DrawCard();
        }

        public async UniTask WaitUserInput()
        {
            await _stageCardsPresenter.WaitUserInput();
        }
    }
}