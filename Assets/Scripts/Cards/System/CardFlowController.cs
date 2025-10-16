using Cardevil.Systems;
using Cysharp.Threading.Tasks;
using Cardevil.Cards.InStage.Model;
using Cardevil.Cards.InStage.Presenter;

namespace Cardevil.Cards
{
    public sealed class CardFlowController : ITurnCardFlow
    {
        private readonly StageCardsModel _model;
        private readonly RerollPresenter _rerollPresenter;
        private readonly StageCardsPresenter _handPresenter;

        private int _maxHand;

        public ITurnRerollInput Reroll => _rerollPresenter;
        public ITurnPlayerInput Hand => _handPresenter;

        public CardFlowController(StageCardsModel model, RerollPresenter rerollPresenter,
            StageCardsPresenter handPresenter)
        {
            _model = model;
            _rerollPresenter = rerollPresenter;
            _handPresenter = handPresenter;
        }

        public async UniTask EnterRerollPhase(int maxHand)
        {
            _maxHand = maxHand;
            
            _rerollPresenter.Init(_model);
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
            _handPresenter.Init(_model);
            await _handPresenter.SetUp(_maxHand);
            DeactivateReroll();
        }

        public async UniTask ExitHandPhase()
        {
            await _handPresenter.Exit();
        }

        public void DeactivateHandPhase()
        {
            _handPresenter.Clear();
        }
    }
}