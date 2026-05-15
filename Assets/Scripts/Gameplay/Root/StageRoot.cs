using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core;
using Cardevil.Core.Attributes;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Dungeon;
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
    public class StageRoot : MonoBehaviour
    {
        [field: Header("References")]
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public Field.Field Field { get; private set; }
        [field: SerializeField, VisibleOnly(EditableIn.EditMode)] public PlayerCharacter Player { get; set; }

        [Space, SerializeField] private StageView view;
        [SerializeField] private StageCardManager cardManager;

        [Space, SerializeField] private ClearRewardTableView rewardTableView;
        [SerializeField] private ClearRewardRelicChestView rewardChestView;
        [SerializeField] private SlotMachine slotMachine;

        [Header("States")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private EnemySpawner _enemySpawner;

        private GameFlowManager.StageEnterContext _context;

        protected void Awake()
        {
            CardevilCore.GameFlow.Stage = this;
        }

        public void Initialize(
            GameFlowManager.StageEnterContext context,
            PlayerStatus playerStatus,
            CardRepository cardRepository,
            ScoreProviderRegistry scoreProviderRegistry)
        {
            _context = context;
            _enemySpawner = new EnemySpawner();
            _enemySpawner.ConfigureStageMobData(CardevilCore.GameFlow.Context.stageId);
            cardManager.Initialize(cardRepository, playerStatus, scoreProviderRegistry);

            Field.InitField(3, 3, 0);
            Player.Init(Field);

            turnManager = new TurnManager(cardManager, Player, _enemySpawner, Field);

            StageCameraCanvas.Instance.InitRock();
        }

        public async UniTask StartStageAsync()
        {
            await view.PlayEnterStageAnimationAsync(_enemySpawner);

            StageCoreLoopAsync().Forget();
        }

        private async UniTask StageCoreLoopAsync()
        {
            await turnManager.CoreLoopAsync();
            
            await PlayShowRewardAsync();

            OnTurnLoopEnded();

            await ExecEventBus<StageLoopEndEventArgs>.InvokeMergedAndDispose(StageLoopEndEventArgs.Get());
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

            LogEx.Log("Stage reward complete");
        }

        private void OnTurnLoopEnded()
        {
            LogEx.Log("Stage ended");

            turnManager?.Dispose();
            turnManager = null;
            _enemySpawner = null;
        }
    }
}
