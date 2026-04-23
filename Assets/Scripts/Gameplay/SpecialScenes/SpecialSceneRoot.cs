using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public abstract class SpecialSceneRoot : MonoBehaviour
    {
        private bool _isClosing;

        protected abstract Scenes SceneType { get; }
        protected abstract SpecialSceneView View { get; }

        protected virtual void Awake()
        {
            CardevilCore.GameFlow.SpecialScene = this;
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(CardevilCore.GameFlow.SpecialScene, this))
            {
                CardevilCore.GameFlow.SpecialScene = null;
            }
        }

        public void Initialize(GameFlowManager.SpecialSceneEnterContext context)
        {
            Bind(context);
            View.CloseRequested -= HandleCloseRequested;
            View.CloseRequested += HandleCloseRequested;
        }

        public async UniTask StartSceneAsync()
        {
            await PlayEnterAsync();
        }

        private void HandleCloseRequested()
        {
            CompleteAndExitAsync().Forget();
        }

        private async UniTask CompleteAndExitAsync()
        {
            if (_isClosing)
            {
                return;
            }

            _isClosing = true;

            var exitStartedArgs = SpecialSceneExitStartedEventArgs.Get();
            exitStartedArgs.Init(SceneType);
            await ExecEventBus<SpecialSceneExitStartedEventArgs>.InvokeMergedAndDispose(exitStartedArgs);

            await PlayExitAsync();

            var args = SpecialSceneLoopEndEventArgs.Get();
            args.Init(SceneType);
            await ExecEventBus<SpecialSceneLoopEndEventArgs>.InvokeMergedAndDispose(args);
        }

        protected abstract void Bind(GameFlowManager.SpecialSceneEnterContext context);
        protected abstract UniTask PlayEnterAsync();
        protected abstract UniTask PlayExitAsync();
    }
}
