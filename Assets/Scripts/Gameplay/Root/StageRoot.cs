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
            /*
            // TODO : 필드 초기화 - @machamy
            Field.InitField(3,3, Random.Range(0,4));
            Player.Init(Field);
            
            _enemySpawner.ConfigureStageMobData(_context.stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. stage Id: {_context.stageId}");
                return;
            }
           
            enemy.Init(Field);
            */


            await view.PlayEnterStageAnimationAsync();
            //
            // await turnManager.CoreLoopAsync();
            //
            // OnTurnLoopEnded();

            await PlayShowRewardAsync();
        }

        private async UniTask PlayShowRewardAsync()
        {
            var rewardPresenter = new StageClearRewardPresenter(
                rewardTableView,
                rewardChestView,
                CardevilCore.PlayerStatus,
                CardevilCore.Game.Relic,
                5,
                DungeonNodeTypes.MiniBoss);
            
            await view.PlayShowDimAsync();
            
            rewardPresenter.Show();
            await rewardPresenter.RewardWaiter.Task;
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
            SceneLoader.UnloadSceneAsync(Scenes.Stage).Forget();
        }
    }
}