using Cysharp.Threading.Tasks;
using System.Threading;

namespace Cardevil.UI.Flow
{
    public sealed class AwaitableUiRequest<T>
    {
        private UniTaskCompletionSource<UiFlowResult<T>> _source;
        private CancellationTokenRegistration _registration;

        public bool IsRunning => _source != null;

        public UniTask<UiFlowResult<T>> Begin(CancellationToken cancellationToken = default)
        {
            Cancel();
            _source = new UniTaskCompletionSource<UiFlowResult<T>>();

            if (cancellationToken.CanBeCanceled)
            {
                _registration = cancellationToken.Register(Cancel);
            }

            return _source.Task;
        }

        public void Complete(T value)
        {
            if (_source == null) return;

            var source = _source;
            Clear();
            source.TrySetResult(UiFlowResult<T>.Completed(value));
        }

        public void Cancel()
        {
            if (_source == null) return;

            var source = _source;
            Clear();
            source.TrySetResult(UiFlowResult<T>.Canceled());
        }

        private void Clear()
        {
            _registration.Dispose();
            _registration = default;
            _source = null;
        }
    }
}
