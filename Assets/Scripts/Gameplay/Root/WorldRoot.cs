using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Systems.Save;
using Cardevil.Gameplay.Dungeon;
using Cardevil.UI.GlobalNavigationBar;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Gameplay.Root
{
    public class WorldRoot : MonoBehaviour, ISaveLoadRoot
    {
        [field: SerializeField] public DungeonManager Dungeon { get; private set; }
        [SerializeField] private WorldView view;
        
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
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            var op = SceneLoader.LoadSceneHandle(Scenes.Stage, LoadSceneMode.Additive);

            var loadReadyTask = UniTask.WaitUntil(() => op.progress >= .9f, cancellationToken: ct);
            var worldAnimTask = view.PlayStageEnterTransitionAsync(dungeon, ct);

            await UniTask.WhenAll(loadReadyTask, worldAnimTask);

            op.allowSceneActivation = true;
            await SceneLoader.WaitSceneActivationAsync(Scenes.Stage, op, LoadSceneMode.Additive, ct);
            SceneLoader.SetActiveScene(Scenes.Stage);

            view.HideMap(dungeon);
        }

        public async UniTask ReturnFromStageAsync(CancellationToken ct)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            CameraManager.Instance.EnableSceneCameras(Scenes.World);
            view.PrepareMapForReturn(dungeon);

            var fadeTask = view.FadeInMapAsync(dungeon, ct);
            var unloadTask = SceneLoader.UnloadSceneAsync(Scenes.Stage);
            var handUpTask = view.PlayReturnToMapTransitionAsync(dungeon, ct);

            await fadeTask;
            await UniTask.WhenAll(unloadTask, handUpTask);

            SceneLoader.SetActiveScene(Scenes.World);
        }
        
        public void Save(GameSave currentSave)
        {
        }

        public void Load(GameSave currentSave)
        {
        }
    }
}
