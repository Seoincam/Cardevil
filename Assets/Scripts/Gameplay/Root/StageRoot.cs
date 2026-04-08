using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon.Core;
using Cardevil.Gameplay.Dungeon.Node;
using Cardevil.Gameplay.Enemy;
using Cardevil.Gameplay.Player;
using Cardevil.Gameplay.Root.Stage;
using Cardevil.Gameplay.Turn;
using Cardevil.UI;
using Cardevil.UI.PopUp;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.Root
{
    /// <summary>
    /// 전투 스테이지 루트 컨트롤러.
    /// 카드, 턴, 적, 필드, 플레이어 초기화 및 전투 흐름 제어.
    /// </summary>
    public class StageRoot : MonoBehaviour
    {
        [field: Header("References")] 
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public Field.Field Field { get; private set; }
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public PlayerCharacter Player { get; set; } // 임시 플레이어
        
        [Space, SerializeField] private StageView view;
        [SerializeField] private StageCardManager cardManager;
        
        [Space, SerializeField] private ClearRewardTableView rewardTableView;
        [SerializeField] private ClearRewardRelicChestView rewardChestView;
        [SerializeField] private SlotMachine slotMachine;

        [Header("States")] 
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EnemySpawner _enemySpawner;
        
        // public int TurnOrder => _turn.TurnOrder;
        
        private GameFlowManager.StageEnterContext _context;


        protected void Awake()
        {
            CardevilCore.GameFlow.Stage = this;
        }

        public void Initialize(
            GameFlowManager.StageEnterContext context,
            PlayerStatus playerStatus,
            CardRepository cardRepository,
            ScoreProviderRegistry scoreProviderRegistry
            )
        {
            _context = context;
            _enemySpawner = new EnemySpawner();
            _enemySpawner.ConfigureStageMobData(CardevilCore.GameFlow.Context.stageId);
            cardManager.Initialize(cardRepository, playerStatus, scoreProviderRegistry);
            
            Field.InitField(3,3,0);// TODO: 스테이지 index 에 따라 받을 수 있게수정하기
            Player.Init(Field);

            turnManager = new TurnManager(cardManager, Player, _enemySpawner, Field);
            
            StageCameraCanvas.Instance.InitRock(); // 돌 끄기
            
            // TODO: 넘겨진 stage 정보에 따라 설정
        }

        
        /// <summary>
        /// 전투 스테이지 진입 비동기 처리.
        /// 스테이지 ID 기반 적 스폰, 필드 및 턴 매니저 구성 후 턴 루프 시작.
        /// </summary>
        public async UniTask EnterStageAsync()
        {
            await view.PlayEnterStageAnimationAsync();

            StageCoreLoopAsync().Forget();
        }


        private async UniTask StageCoreLoopAsync()
        {
            // await turnManager.CoreLoopAsync();
            
            // TODO: 이기는 경우만 일단 처리중. 패배 고려해야함.
            await PlayShowRewardAsync();
            
            OnTurnLoopEnded();
        }

        private async UniTask PlayShowRewardAsync()
        {
            var rewardPresenter = new StageClearRewardPresenter(
                rewardTableView,
                rewardChestView,
                slotMachine,
                CardevilCore.PlayerStatus,
                CardevilCore.Game.Relic,
                5,
                DungeonNodeTypes.MiniBoss);
            
            await view.PlayShowDimAsync();
            
            rewardPresenter.Show();
            await rewardPresenter.RewardWaiter.Task;
            
            await view.PlayHideDimAsync();
            
            LogEx.Log("스테이지 보상 획득 끝!");
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
            
            CardevilCore.GameFlow.Stage = null;
            
            turnManager?.Dispose();
            turnManager = null;

            // _enemySpawner?.Dispose();
            _enemySpawner = null;
            
            var exitInfo = new NodeExitInfo() { IsCleared = true };
            CardevilCore.GameFlow.World.Dungeon.ExitCurrentNode(exitInfo);
            CardevilCore.GameFlow.World.Dungeon.UI.gameObject.SetActive(true); // 임시로 껐던거 임시로 여기서 킴
            SceneLoader.UnloadSceneAsync(Scenes.Stage).Forget();
        }
    }
}