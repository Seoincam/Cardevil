using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.InWorld.UI;
using Cardevil.Card.InWorld.UI.Upgrade;
using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Utils;
using Cardevil.Gameplay.Root.Stage;
using Cardevil.Gameplay.Relics.Core;
using Cardevil.UI.Flow;
using Cysharp.Threading.Tasks;
using UnityEngine;
using NavigationBar = Cardevil.UI.GlobalNavigationBar.GlobalNavigationBar;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopRoot : SpecialSceneRoot
    {
        private const string UpgradeDatabasePath = "ScriptableObjects/NewCard/UpgradeNodes/UpgradeNodeDatabase";
        private const string UpgradeMainText = "골드를 지불해 선택한 카드를 다음 강화 단계로 확정 강화합니다.";

        [SerializeField] private ShopView view;
        [SerializeField] private UpgradeNodeDatabaseSO upgradeDatabase;

        private readonly ShopCore _core = new();
        private CardWorldUiHost _uiHost;
        private CardUpgradePresenter _upgradePresenter;
        private NavigationBar _navigationBar;
        private bool _createdNavigationBar;
        private bool _isShopActionRunning;

        protected override Scenes SceneType => Scenes.Shop;
        protected override SpecialSceneView View => view;

        protected override void Bind(GameFlowManager.SpecialSceneEnterContext context)
        {
            upgradeDatabase ??= AssetUtil.Load<UpgradeNodeDatabaseSO>(UpgradeDatabasePath);
            if (!upgradeDatabase)
            {
                LogEx.LogError($"Upgrade database not found: {UpgradeDatabasePath}");
            }

            _uiHost ??= CardWorldUiHost.Instantiate();
            if (!_uiHost || !_uiHost.UpgradeView)
            {
                LogEx.LogError($"{nameof(CardWorldUiHost)} prefab is missing an upgrade view.");
            }

            if (upgradeDatabase && _uiHost && _uiHost.UpgradeView)
            {
                _upgradePresenter?.Dispose();
                _upgradePresenter = new CardUpgradePresenter(
                    CardevilCore.Game.CardRepository,
                    upgradeDatabase,
                    _uiHost.UpgradeView);
                view.SetCardEntryPrefab(_uiHost.UpgradeView.CardPrefab);
            }

            _core.Configure(upgradeDatabase);
            _core.Initialize(context);

            view.EntryClicked -= HandleEntryClicked;
            view.EntryClicked += HandleEntryClicked;
            view.Bind(_core);

            _navigationBar = EnsureGlobalNavigationBar();
            _navigationBar?.Show();
        }

        protected override void OnDestroy()
        {
            if (view)
            {
                view.EntryClicked -= HandleEntryClicked;
            }

            _upgradePresenter?.Dispose();
            _upgradePresenter = null;

            if (_uiHost)
            {
                Destroy(_uiHost.gameObject);
                _uiHost = null;
            }

            if (_createdNavigationBar && _navigationBar)
            {
                Destroy(_navigationBar.gameObject);
                _navigationBar = null;
            }

            base.OnDestroy();
        }

        private void HandleEntryClicked(ShopEntryData entry)
        {
            RunShopEntryAsync(entry).Forget();
        }

        private async UniTaskVoid RunShopEntryAsync(ShopEntryData entry)
        {
            if (_isShopActionRunning || !entry.IsAvailable)
            {
                return;
            }

            _isShopActionRunning = true;
            view.SetEntriesInteractable(false);

            try
            {
                switch (entry.Kind)
                {
                    case ShopEntryKind.CardReinforce:
                        await RunCardReinforceEntryAsync(entry);
                        break;

                    case ShopEntryKind.Consumable:
                        HandleConsumableEntry(entry);
                        break;
                }
            }
            finally
            {
                _isShopActionRunning = false;
                if (view)
                {
                    view.SetEntriesInteractable(true);
                }

                if (_uiHost && entry.Kind == ShopEntryKind.CardReinforce)
                {
                    _uiHost.SetMainUi(null, null);
                }
            }
        }

        private async UniTask RunCardReinforceEntryAsync(ShopEntryData entry)
        {
            if (_upgradePresenter == null)
            {
                LogEx.LogError("Card upgrade presenter is not initialized.");
                return;
            }

            if (_uiHost)
            {
                _uiHost.SetMainUi(null, UpgradeMainText);
            }

            var result = await _upgradePresenter.RequestUpgradeAsync(
                entry.CardSpecId,
                node => entry.Discount.Apply(node.MarketCost),
                this.GetCancellationTokenOnDestroy());

            if (result.Status != UiFlowStatus.Completed)
            {
                return;
            }

            _core.RefreshCardReinforceEntry(result.Value);
            view.Bind(_core);
            LogEx.Log($"Reinforced card: {result.Value}");
        }

        private void HandleConsumableEntry(ShopEntryData entry)
        {
            if (CardevilCore.PlayerStatus[StatType.Gold] < entry.FinalGoldCost)
            {
                LogEx.Log($"Not enough gold for shop entry: {entry.TooltipKey}");
                return;
            }

            _navigationBar = _navigationBar ? _navigationBar : EnsureGlobalNavigationBar();
            if (!_navigationBar)
            {
                LogEx.LogError("Cannot purchase consumable because GlobalNavigationBar is missing.");
                return;
            }

            if (!_navigationBar.HasConsumableSlot)
            {
                LogEx.Log("Cannot purchase consumable because every GNB item slot is occupied.");
                return;
            }

            var icon = entry.LoadIcon();
            if (!icon)
            {
                LogEx.LogWarning($"Shop consumable icon not found: {entry.IconResourcePath}");
            }

            bool added = _navigationBar.TryAddConsumable(
                icon,
                ShopView.ResolveShopTooltip(entry),
                () => UseShopConsumable(entry));
            if (!added)
            {
                LogEx.Log("Cannot purchase consumable because every GNB item slot is occupied.");
                return;
            }

            CardevilCore.PlayerStatus.ModifyBaseValue(StatType.Gold, -entry.FinalGoldCost);
            _navigationBar.RefreshStatusTexts();
            _core.MarkConsumablePurchased(entry);
            view.Bind(_core);
            LogEx.Log($"Purchased shop entry: {entry.TooltipKey}");
        }

        protected override UniTask PlayEnterAsync()
        {
            _navigationBar = _navigationBar ? _navigationBar : EnsureGlobalNavigationBar();
            _navigationBar?.Show();
            return view.PlayEnterAsync();
        }

        protected override UniTask PlayExitAsync()
        {
            return view.PlayExitAsync();
        }

        private NavigationBar EnsureGlobalNavigationBar()
        {
            var navigationBar = NavigationBar.Instance;
            if (navigationBar)
            {
                return navigationBar;
            }

            var navigationBarObject = AssetUtil.Instantiate("UI/GNB UI");
            if (!navigationBarObject)
            {
                LogEx.LogError("GlobalNavigationBar prefab was not found at Prefabs/UI/GNB UI.");
                return null;
            }

            _createdNavigationBar = true;
            navigationBar = navigationBarObject.GetComponent<NavigationBar>();
            if (!navigationBar)
            {
                LogEx.LogError("GNB UI prefab does not have GlobalNavigationBar.");
            }

            return navigationBar;
        }

        private static void UseShopConsumable(ShopEntryData entry)
        {
            if (CardevilCore.Instance == null || CardevilCore.Instance.GameManager == null)
            {
                return;
            }

            var playerStatus = CardevilCore.Instance.GameManager.PlayerStatus;
            switch (entry.ConsumableKind)
            {
                case ShopConsumableKind.GreenCard:
                    playerStatus.Heal(1);
                    break;

                case ShopConsumableKind.RedCard:
                    RegisterOneShotDamageMultiplier(1.5f, "Red Card");
                    break;

                case ShopConsumableKind.BlueCard:
                    playerStatus.ModifyBaseValue(StatType.RerollTicket, 1);
                    break;

                case ShopConsumableKind.BlackCard:
                    ApplyBlackCardRoll(playerStatus);
                    break;

                case ShopConsumableKind.RelicChest:
                    OpenRelicChestSelectionAsync().Forget();
                    break;
            }

            NavigationBar.Instance?.RefreshStatusTexts();
            LogEx.Log($"Used shop consumable: {entry.TooltipKey}");
        }

        private static void ApplyBlackCardRoll(PlayerStatus playerStatus)
        {
            int roll = UnityEngine.Random.Range(1, 7);
            switch (roll)
            {
                case 6:
                    playerStatus.ModifyBaseValue(StatType.Gold, 6);
                    break;

                case 5:
                    playerStatus.Heal(1);
                    break;

                case 4:
                    RegisterOneShotDamageMultiplier(2f, "Black Card");
                    break;

                case 3:
                    playerStatus.ModifyBaseValue(StatType.RerollTicket, 1);
                    break;

                case 2:
                    playerStatus.ModifyBaseValue(StatType.Shield, 1);
                    break;

                default:
                    playerStatus.TakeDamage(1);
                    break;
            }

            LogEx.Log($"Black Card roll: {roll}");
        }

        private static void RegisterOneShotDamageMultiplier(float multiplier, string sourceName)
        {
            if (CardevilCore.Instance == null || CardevilCore.Instance.GameManager == null)
            {
                return;
            }

            var provider = new OneShotDamageMultiplier(multiplier, sourceName);
            provider.Id = CardevilCore.Game.ScoreProviderRegistry.Register(provider);
        }

        private sealed class OneShotDamageMultiplier : IScoreProvider
        {
            private readonly float _multiplier;
            private readonly string _sourceName;

            public OneShotDamageMultiplier(float multiplier, string sourceName)
            {
                _multiplier = multiplier;
                _sourceName = sourceName;
                Id = -1;
            }

            public int Id { get; set; }
            public ScoreStepType ScoreStepType => ScoreStepType.MultiplyPlayerStatus;

            public IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (Id >= 0)
                {
                    CardevilCore.Game.ScoreProviderRegistry.SafeUnregister(Id, this);
                    Id = -1;
                }

                return new ScoreOperator
                {
                    Source = this,
                    Type = ScoreOperatorType.Multiply,
                    Value = _multiplier
                };
            }

            public override string ToString()
            {
                return _sourceName;
            }
        }

        private static async UniTaskVoid OpenRelicChestSelectionAsync()
        {
            var relicManager = CardevilCore.Instance?.GameManager?.Relic;
            if (relicManager == null)
            {
                return;
            }

            var relics = relicManager.GetRandomUnownedRelicsByRarity(RelicRarity.Default, 3);
            if (relics.Count < 3)
            {
                AddRandomDefaultRelic();
                return;
            }

            Transform parent = NavigationBar.Instance ? NavigationBar.Instance.transform : null;
            var viewObject = AssetUtil.Instantiate("UI/Stage Clear Reward Relic Chest UI", parent);
            if (!viewObject || !viewObject.TryGetComponent(out ClearRewardRelicChestView view))
            {
                if (viewObject)
                {
                    Destroy(viewObject);
                }

                AddRandomDefaultRelic();
                return;
            }

            var completion = new UniTaskCompletionSource();
            bool relicSelected = false;

            void HandleRelicClicked(int index)
            {
                if (relicSelected)
                {
                    return;
                }

                if (index < 0 || index >= relics.Count || relics[index] == null)
                {
                    return;
                }

                relicSelected = true;
                relicManager.AddRelic(relics[index]);
                completion.TrySetResult();
            }

            void HandleRerollClicked(int index)
            {
                if (relicSelected)
                {
                    return;
                }

                var rerolled = relicManager.RerollSingleRelic(RelicRarity.Default, relics, index);
                if (rerolled != null)
                {
                    view.RefreshRelic(index, rerolled);
                }
            }

            view.RelicClicked += HandleRelicClicked;
            view.RerollClicked += HandleRerollClicked;

            try
            {
                await view.PlayShowAnimationAsync(relics);
                await completion.Task;
                await view.PlayHideAnimationAsync();
            }
            finally
            {
                view.RelicClicked -= HandleRelicClicked;
                view.RerollClicked -= HandleRerollClicked;
                Destroy(viewObject);
            }
        }

        private static void AddRandomDefaultRelic()
        {
            var relicManager = CardevilCore.Instance.GameManager.Relic;
            if (relicManager == null)
            {
                return;
            }

            var relics = relicManager.GetRandomUnownedRelicsByRarity(RelicRarity.Default, 1);
            if (relics.Count > 0)
            {
                relicManager.AddRelic(relics[0]);
            }
        }
    }
}
