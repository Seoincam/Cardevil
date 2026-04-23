using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Systems.Save;
using Cardevil.Gameplay.Dungeon;
using Cardevil.Gameplay.SpecialScenes;
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

            if (Dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager reference is missing.");
                return;
            }

            Dungeon.Init();

            GlobalNavigationBar.Instance.gameObject.SetActive(true);
            Dungeon.UI.UpdateShowingDungeon(1);
        }

        public async UniTask TransitionToStageAsync(GameFlowManager.StageEnterContext ctx, CancellationToken ct = default)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            var op = SceneLoader.LoadSceneHandle(Scenes.Stage, LoadSceneMode.Additive, activateOnLoad: false);
            
            var loadReadyTask = UniTask.WaitUntil(() => op.progress >= .9f, cancellationToken: ct);
            var worldAnimTask = view.PlayStageEnterTransitionAsync(dungeon, ct);

            await UniTask.WhenAll(loadReadyTask, worldAnimTask);
            
            op.allowSceneActivation = true;
            
            await op;
            
            SceneLoader.SetActiveScene(Scenes.Stage);
            CameraManager.Instance.EnableSceneCameras(Scenes.Stage);
            view.ResetChapterTransform(dungeon);

            view.HideMap(dungeon);
        }

        public async UniTask TransitionToSpecialSceneAsync(GameFlowManager.SpecialSceneEnterContext ctx, CancellationToken ct = default)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            var op = SceneLoader.LoadSceneHandle(ctx.scene, LoadSceneMode.Additive, activateOnLoad: false);
            var loadReadyTask = UniTask.WaitUntil(() => op.progress >= .9f, cancellationToken: ct);
            var worldAnimTask = view.PlaySpecialScenePreludeAsync(dungeon, ct);

            await UniTask.WhenAll(loadReadyTask, worldAnimTask);

            op.allowSceneActivation = true;
            await op;

            SceneLoader.SetActiveScene(ctx.scene);
            CameraManager.Instance.EnableSceneCameras(ctx.scene);
            view.HideMap(dungeon);

            var root = CardevilCore.GameFlow.SpecialScene;
            if (root == null)
            {
                Debug.LogError($"WorldRoot: Special scene root not found for {ctx.scene}.");
                return;
            }

            if (root.gameObject.scene.name != ctx.scene.GetSceneName())
            {
                Debug.LogError($"WorldRoot: Loaded special scene root scene mismatch. Expected {ctx.scene}, got {root.gameObject.scene.name}.");
                return;
            }

            root.Initialize(ctx);
            await root.StartSceneAsync();
        }

        public async UniTask ReturnToWorldFromStageAsync(CancellationToken ct)
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


            await fadeTask;
            var unloadTask = SceneLoader.UnloadSceneAsync(Scenes.Stage);
            var handUpTask = view.PlayReturnToMapTransitionAsync(dungeon, ct);
            await UniTask.WhenAll(unloadTask, handUpTask);

            SceneLoader.SetActiveScene(Scenes.World);
        }

        public UniTask PrepareWorldForSpecialSceneReturnAsync(Scenes scene, CancellationToken ct)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return UniTask.CompletedTask;
            }

            view.PrepareMapBehindSpecialScene(dungeon);
            view.PrepareChapterVisualForSpecialSceneReturn(dungeon);
            CameraManager.Instance.EnableSceneCameras(Scenes.World);
            return UniTask.CompletedTask;
        }

        public async UniTask ReturnToWorldFromSpecialSceneAsync(Scenes scene, CancellationToken ct)
        {
            var dungeon = Dungeon;
            if (dungeon == null)
            {
                Debug.LogError("WorldRoot: Dungeon manager is not initialized.");
                return;
            }

            CameraManager.Instance.DisableSceneCameras(scene);
            await view.PlayReturnToMapTransitionAsync(dungeon, ct);
            await SceneLoader.UnloadSceneAsync(scene);
            SceneLoader.SetActiveScene(Scenes.World);
            view.EnableMapInteraction(dungeon);
        }
        
        public void Save(GameSave currentSave)
        {
        }

        public void Load(GameSave currentSave)
        {
        }
    }
}
