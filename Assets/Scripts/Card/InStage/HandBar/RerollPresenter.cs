using Cardevil.Gameplay;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public class RerollPresenter
    {
        private readonly PlayerStatus _playerStatus;
        private readonly RerollView _view;
        private readonly StageCardCorePresenter _corePresenter;
        private readonly HandBarPresenter _handBarPresenter;

        private UniTaskCompletionSource _rerollWaiter;

        private int RerollTicketCount => _playerStatus.GetFinalValue(PlayerStatType.RerollTicket);
        private bool CanReroll => RerollTicketCount > 0;

        public RerollPresenter(PlayerStatus playerStatus, RerollView view, StageCardCorePresenter corePresenter, HandBarPresenter handBarPresenter)
        {
            _playerStatus = playerStatus;
            _view = view;
            _corePresenter = corePresenter;
            _handBarPresenter = handBarPresenter;

            view.RerollClicked += OnRerollRequested;
            view.ConfirmClicked += OnConfirmRequested;
        }

        public async UniTask WaitUntilRerollEndAsync()
        {
            _rerollWaiter = new UniTaskCompletionSource();
            
            _view.SetTicketCount(RerollTicketCount);
            await DrawAsync();
            await _rerollWaiter.Task;
            
            _rerollWaiter = null;
        }

        public async UniTask DrawAsync()
        {
            await _corePresenter.DrawAsync();
        }

        private async UniTask DiscardToDeckAsync()
        {
            var states = await _handBarPresenter.RerollAllCardToDeck();
            _corePresenter.Reroll(states);
        }

        private void OnRerollRequested()
        {
            Debug.Assert(CanReroll, "Reroll Ticket Count > 0");
            
            _playerStatus.ModifyBaseValue(PlayerStatType.RerollTicket, -1);
            _view.SetTicketCount(RerollTicketCount);
            
            async UniTask RerollAsync()
            {
                _view.SetConfirmButton(false);
                _view.SetRerollButton(false);
                
                await DiscardToDeckAsync();
                await DrawAsync();
                
                _view.SetConfirmButton(true);
                if (RerollTicketCount > 0) _view.SetRerollButton(true);
            }
            RerollAsync().Forget();
        }

        private void OnConfirmRequested()
        {
            _rerollWaiter.TrySetResult();
            _view.gameObject.SetActive(false);
        }
    }
}