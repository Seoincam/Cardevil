using Cysharp.Threading.Tasks;
using System;

namespace Cardevil.Card.InStage
{
    public class RerollPresenter
    {
        private readonly StageCardCorePresenter _corePresenter;
        private readonly HandBarPresenter _handBarPresenter;

        private UniTaskCompletionSource _rerollWaiter;

        public RerollPresenter(StageCardCorePresenter corePresenter, HandBarPresenter handBarPresenter)
        {
            _corePresenter = corePresenter;
            _handBarPresenter = handBarPresenter;
        }

        public async UniTask WaitUntilRerollEndAsync()
        {
            _rerollWaiter = new UniTaskCompletionSource();
            
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

        // TODO: 임시로 public으로 둔 함수 private로 전환하기.
        public void OnRerollRequested()
        {
            async UniTask RerollAsync()
            {
                await DiscardToDeckAsync();
                await DrawAsync();
            }
            RerollAsync().Forget();
        }

        public void OnConfirmRequested()
        {
            _rerollWaiter.TrySetResult();
        }
    }
}