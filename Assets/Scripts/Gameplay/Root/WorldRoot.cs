using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Systems.Save;
using Cardevil.Gameplay.Dungeon;
using Cardevil.UI.GlobalNavigationBar;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Gameplay.Root
{
    public class WorldRoot : MonoBehaviour, ISaveLoadRoot
    {
        [field: SerializeField] public DungeonManager Dungeon { get; private set; }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            CardevilCore.GameFlow.World = this;

            Dungeon = new DungeonManager();
            Dungeon.Init();

            GlobalNavigationBar.Instance.gameObject.SetActive(true);
            Dungeon.UI.UpdateShowingDungeon(1);
        }

        public async UniTask EnterStageAsync(GameFlowManager.StageEnterContext ctx, CancellationToken ct = default)
        {
            var op = SceneLoader.LoadSceneHandle(Scenes.Stage, LoadSceneMode.Additive);

            var loadReadyTask = UniTask.WaitUntil(() => op.progress >= .9f, cancellationToken: ct);
            var worldAnimTask = PlayStageEnterAnimation(ct);

            await UniTask.WhenAll(loadReadyTask, worldAnimTask);

            op.allowSceneActivation = true;
            await SceneLoader.WaitSceneActivationAsync(Scenes.Stage, op, LoadSceneMode.Additive, ct);
            SceneLoader.SetActiveScene(Scenes.Stage);

            Dungeon.UI.gameObject.SetActive(false);
        }

        public async UniTask ReturnFromStageAsync(CancellationToken ct)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            dungeon.UI.gameObject.SetActive(true);
            dungeon.UI.CanvasGroup.alpha = 0f;
            CameraManager.Instance.EnableSceneCameras(Scenes.World);

            await dungeon.UI.CanvasGroup.DOFade(1f, 0.2f).ToUniTask(cancellationToken: ct);

            var unloadTask = SceneLoader.UnloadSceneAsync(Scenes.Stage);
            var handUpTask = PlayReturnToMapAnimation(ct);
            await UniTask.WhenAll(unloadTask, handUpTask);

            SceneLoader.SetActiveScene(Scenes.World);
        }

        private async UniTask PlayStageEnterAnimation(CancellationToken ct)
        {
            var transitionUI = Dungeon.Transition;
            if (transitionUI)
            {
                await transitionUI.ShowEnterTransition(ct);
            }
        }

        private async UniTask PlayReturnToMapAnimation(CancellationToken ct)
        {
            var transitionUI = Dungeon.Transition;
            if (transitionUI)
            {
                await transitionUI.ShowHandUpAnimation(ct);
            }
        }

        public void Save(GameSave currentSave)
        {
        }

        public void Load(GameSave currentSave)
        {
        }
    }
}
