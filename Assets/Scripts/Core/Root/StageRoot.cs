using Cardevil.Attributes;
using Cardevil.Cards.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn;
using Cardevil.Dungeon;
using Cardevil.Enemy;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Cardevil.Ingame.Field;
using Cardevil.Ingame.Player;
using Cardevil.SceneManagement;
using Cardevil.UI;
using Cardevil.UI.GlobalNavationBar;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Cardevil.Core.Root
{
    /// <summary>
    /// 전투 스테이지 루트 컨트롤러.
    /// 카드, 턴, 적, 필드, 플레이어 초기화 및 전투 흐름 제어.
    /// </summary>
    public class StageRoot : MonoBehaviour
    {
        [SerializeField] private TurnManager turn;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private CardFlowController cardFlowController;

        [Header("References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public Field Field { get; private set; }
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public PlayerCharacter Player { get; set; } // 임시 플레이어

        // public int TurnOrder => _turn.TurnOrder;
        
        private GameFlowManager.StageEnterContext _context;

        bool isInitialized = false;
        
        private async void Awake()
        {
            CardevilCore.Instance.GameFlow.Stage = this;
            
            // TODO: 로딩을 bootstrapper or stage에서 관리할지 고민하기
            
            _enemySpawner = new EnemySpawner();
            turn = new TurnManager();
            // turn.TurnLoopEnded += OnTurnLoopEnded;
            
            Player.Init(Field);
            await InitAsync();
            
        }

        private async UniTask InitAsync()
        {
            _context = CardevilCore.Instance.GameFlow.Context;
            
            StageCameraCanvas.Instance.InitRock(); // 돌 끄기
            
            isInitialized = true;
        }
        
        /// <summary>
        /// 전투 스테이지 진입 비동기 처리.
        /// 스테이지 ID 기반 적 스폰, 필드 및 턴 매니저 구성 후 턴 루프 시작.
        /// </summary>
        public async UniTask EnterStageAsync()
        {
            // TODO: 얘를 외부에서 호출되도록 해야겠음.
            // @machamy : GameFlow -> StageRoot.EnterStageAsync() 이렇게 해둠
            await UniTask.WaitUntil(() => isInitialized);
            
            CameraManager.Instance.DisableSceneCameras(Scenes.World);
            

            
            
            // TODO : 필드 초기화 - @machamy
            Field.InitField(3,3, Random.Range(0,4));
            
            _enemySpawner.ConfigStageMobData(_context.stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. stage Id: {_context.stageId}");
                return;
            }
            
            cardFlowController = CardFlowController.Build();
            
            enemy.Init(Field);
            
            // 페이드아웃 + 돌 가져오기
            var blakcFade = OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.8f)
                .ToUniTask(TweenCancelBehaviour.Complete);

            var rock = StageCameraCanvas.Instance.AnimShowRock(0.8f);
            
            await UniTask.WhenAll(blakcFade, rock);
            
            // TODO : 가운데에 적 정보 보이기

            await UniTask.Delay(300);
            
            // GNB 보이기
            GlobalNavigationBar gnb = GlobalNavigationBar.Instance;
            gnb.gameObject.SetActive(true);
            await gnb.ShowAsync(0.4f);
            
            
            turn.Initialize(_enemySpawner, cardFlowController, Player, enemy);
            turn.EnterLoopAsync().Forget();
        }

        /// <summary>
        /// 턴 루프 종료 처리.
        /// 던전 노드 퇴장 처리 및 스테이지 씬 언로드.
        /// </summary>
        private void OnTurnLoopEnded()
        {
            // TODO: 보상창 등 나와야함.
            // TODO: 변경사항 세이브 해야함.
            
            LogEx.Log("스테이지 종료");
            
            var exitInfo = new NodeExitInfo() { IsCleared = true };
            CardevilCore.Instance.GameFlow.World.Dungeon.ExitCurrentNode(exitInfo);
            SceneLoader.UnloadSceneAsync(Scenes.Stage).Forget();
        }
    }
}