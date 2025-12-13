using Cardevil.Core.Bootstrap;
using Cardevil.Dungeon;
using Cardevil.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Root
{
    public class WorldRoot : MonoBehaviour
    {
        public static WorldRoot Instance { get; private set; }
        
        [field: SerializeField] public DungeonManager Dungeon { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            Init();
        }

        public void Init()
        {
            Bootstrapper.Instance.GameFlow.World = this;
            
            Dungeon = new DungeonManager();
            Dungeon.Init();
        }

        public async UniTask EnterStageAsync(GameFlowManager.StageEnterContext ctx, CancellationToken ct = default)
        {
            // 씬 로드 & 스테이지 선택 연출까지 대기
            var op = SceneLoader.LoadSceneHandle(Scenes.Stage, LoadSceneMode.Additive);

            var loadReadyTask = UniTask.WaitUntil(() => op.progress >= .9f, cancellationToken: ct);
            var worldAnimTask = PlayDungeonEnterAnimation(ct);

            await UniTask.WhenAll(loadReadyTask, worldAnimTask);

            // 씬 활성화 허용
            op.allowSceneActivation = true;
            await SceneLoader.WaitSceneActivationAsync(Scenes.Stage, op, LoadSceneMode.Additive, ct);
            SceneLoader.SetActiveScene(Scenes.Stage);
            
            // TODO: StageRoot에 알리기
        }

        private async UniTask PlayDungeonEnterAnimation(CancellationToken ct)
        {
            // TODO: 던전 입장 연출
        }
    }
}