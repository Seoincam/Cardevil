using Cardevil.Core.Bootstrap;
using Cardevil.Dungeon;
using Cardevil.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cardevil.Core.Root
{
    /// <summary>
    /// 월드 루트 컨트롤러.
    /// 던전 관리 및 스테이지 진입 흐름 제어.
    /// </summary>
    public class WorldRoot : MonoBehaviour
    {
        [field: SerializeField] public DungeonManager Dungeon { get; private set; }

        private void Awake()
        {
            Init();
        }

        
        /// <summary>
        /// 월드 초기화.
        /// GameFlow 월드 설정 및 던전 매니저 생성·초기화.
        /// </summary>
        public void Init()
        {
            Bootstrapper.Instance.GameFlow.World = this;
            
            Dungeon = new DungeonManager();
            Dungeon.Init();
        }

        /// <summary>
        /// 스테이지 진입 비동기 처리.
        /// 씬 로드 진행 대기 및 던전 입장 연출 병행 후 씬 활성화.
        /// </summary>
        /// <param name="ctx">스테이지 진입 컨텍스트</param>
        /// <param name="ct">취소 토큰</param>
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

        
        /// <summary>
        /// 던전 입장 연출 실행.
        /// 스테이지 진입 전 월드 연출 처리.
        /// </summary>
        /// <param name="ct">취소 토큰</param>
        private async UniTask PlayDungeonEnterAnimation(CancellationToken ct)
        {
            // TODO: 던전 입장 연출
        }
    }
}