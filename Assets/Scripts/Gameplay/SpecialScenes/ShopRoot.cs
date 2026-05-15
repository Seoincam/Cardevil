using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.InWorld.UI;
using Cardevil.Core;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.SceneManagement;
using Cardevil.Core.Utils;
using Cardevil.UI.Flow;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopRoot : SpecialSceneRoot
    {
        private const string UpgradeDatabasePath = "ScriptableObjects/NewCard/UpgradeNodes/UpgradeNodeDatabase";

        [SerializeField] private ShopView view;
        [SerializeField] private UpgradeNodeDatabaseSO upgradeDatabase;

        private readonly ShopCore _core = new();
        private bool _isReinforceFlowRunning;

        protected override Scenes SceneType => Scenes.Shop;
        protected override SpecialSceneView View => view;

        protected override void Bind(GameFlowManager.SpecialSceneEnterContext context)
        {
            _core.Initialize(context);
            view.ReinforceRequested -= HandleReinforceRequested;
            view.ReinforceRequested += HandleReinforceRequested;
            view.Bind(_core);
        }

        protected override void OnDestroy()
        {
            if (view)
            {
                view.ReinforceRequested -= HandleReinforceRequested;
            }

            base.OnDestroy();
        }

        private void HandleReinforceRequested()
        {
            RunReinforceFlowAsync().Forget();
        }

        private async UniTaskVoid RunReinforceFlowAsync()
        {
            if (_isReinforceFlowRunning)
            {
                return;
            }

            _isReinforceFlowRunning = true;
            view.SetReinforceInteractable(false);

            try
            {
                upgradeDatabase ??= AssetUtil.Load<UpgradeNodeDatabaseSO>(UpgradeDatabasePath);
                if (!upgradeDatabase)
                {
                    LogEx.LogError($"Upgrade database not found: {UpgradeDatabasePath}");
                    return;
                }

                var flow = new CardUpgradeUiFlow(
                    CardevilCore.Game.CardRepository,
                    upgradeDatabase,
                    _core.ReinforceDrawCount);

                var result = await flow.SelectAndUpgradeAsync(this.GetCancellationTokenOnDestroy());
                if (result.Status == UiFlowStatus.Completed)
                {
                    LogEx.Log($"Reinforced card: {result.Value}");
                }
            }
            finally
            {
                _isReinforceFlowRunning = false;
                if (view)
                {
                    view.SetReinforceInteractable(true);
                }
            }
        }

        protected override UniTask PlayEnterAsync()
        {
            return view.PlayEnterAsync();
        }

        protected override UniTask PlayExitAsync()
        {
            return view.PlayExitAsync();
        }
    }
}
