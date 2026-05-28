using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Utils;
using Cardevil.Gameplay;
using Cardevil.UI.Flow;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Cardevil.Card.InWorld.UI.Upgrade
{
    public class CardUpgradePresenter : IDisposable
    {
        /// <summary>
        /// 카드가 업그레이드됐을 때 발행되는 이벤트. 해당 카드의 ID를 인자로 함.
        /// </summary>
        public event Action<int> CardUpgraded;
        
        
        private readonly CardRepository _repository;
        private readonly UpgradeNodeDatabaseSO _upgradeDatabase;
        private readonly CardUpgradeView _view;
        private readonly AwaitableUiRequest<int> _upgradeRequest = new();
        
        private int _targetSpecId;
        private IReadOnlyList<UpgradeNodeSO> _availableNodes;
        private Func<UpgradeNodeSO, int> _costResolver;


        public struct UpgradeData
        { 
            public readonly UpgradeNodeSO UpgradeNode;
            public readonly CardVisualInput VisualInput;
            public readonly int Cost;
            public readonly int OriginalCost;

            public bool HasDiscount => Cost < OriginalCost;

            public UpgradeData(UpgradeNodeSO upgradeNode, CardVisualInput visualInput, int cost)
                : this(upgradeNode, visualInput, cost, cost)
            {
            }

            public UpgradeData(UpgradeNodeSO upgradeNode, CardVisualInput visualInput, int cost, int originalCost)
            {
                UpgradeNode = upgradeNode;
                VisualInput = visualInput;
                Cost = cost;
                OriginalCost = Math.Max(cost, originalCost);
            }
        }
        
        
        public CardUpgradePresenter(CardRepository repository, UpgradeNodeDatabaseSO upgradeDatabase, CardUpgradeView view)
        {
            _repository = repository;
            _upgradeDatabase = upgradeDatabase;
            _view = view;

            _view.SelectedNodeChanged += HandleSelectedNodeChanged;
            _view.CloseClicked += HandleCloseClicked;
            _view.ConfirmClicked += HandleUpgradeConfirmClicked;
        }

        
        /// <summary>
        /// 선택 창에서 카드가 선택됐을 때 호출되는 콜백.
        /// </summary>
        public void HandleCardSelected(int specId)
        {
            var spec = _repository.GetSpec(specId);
            if (spec == null)
            {
                LogEx.LogError($"강화할 카드를 찾을 수 없습니다. ID: {specId}");
                return;
            }

            var availableUpgradeNodes = _upgradeDatabase.GetNextAvailableNodes(spec);

            if (availableUpgradeNodes.Count == 0)
            {
                LogEx.LogError($"가능한 강화 노드가 존재하지 않음. ID: {specId}");
                return;
            }

            if (availableUpgradeNodes.Count > 2)
            {
                LogEx.LogError($"가능한 강화 노드 수가 UI 허용 범위를 초과했습니다. Count: {availableUpgradeNodes.Count}");
                return;
            }
            
            Open(specId, availableUpgradeNodes);
        }
        
        public void Open(int specId, IReadOnlyList<UpgradeNodeSO> availableNodes)
        {
            if (availableNodes == null || availableNodes.Count > 2 || availableNodes.Count <= 0)
            {
                LogEx.LogError($"가능한 강화 노드 수가 적절하지 않습니다. Count: {availableNodes?.Count ?? 0}");
                return;
            }
            
            _targetSpecId = specId;
            _availableNodes = availableNodes;
            
            
            var originalState = _repository.GetNewState(specId);
            var originalVisual = CardVisualInput.From(originalState);

            switch (_availableNodes.Count)
            {
                case 1 :
                    var nextSpec = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[0]);
                    var nextVisual = CardVisualInput.From(nextSpec);
                    var data = CreateUpgradeData(availableNodes[0], nextVisual);
                    
                    _view.Create(originalVisual, data);
                    break;
                
                case 2: 
                    var nextSpec1 = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[0]);
                    var nextVisual1 = CardVisualInput.From(nextSpec1.State);
                    var data1 = CreateUpgradeData(availableNodes[0], nextVisual1);

                    var nextSpec2 = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[1]);
                    var nextVisual2 = CardVisualInput.From(nextSpec2.State);
                    var data2 = CreateUpgradeData(availableNodes[1], nextVisual2);

                
                    _view.Create(originalVisual, data1, data2);
                    break;
            }
            
            _view.PlayOpenAnimationAsync().Forget();
        }

        private UpgradeData CreateUpgradeData(UpgradeNodeSO node, CardVisualInput visualInput)
        {
            return new UpgradeData(node, visualInput, ResolveCost(node), node.MarketCost);
        }

        public async UniTask<UiFlowResult<int>> RequestUpgradeAsync(int specId, CancellationToken cancellationToken = default)
        {
            return await RequestUpgradeAsync(specId, null, cancellationToken);
        }

        public async UniTask<UiFlowResult<int>> RequestUpgradeAsync(
            int specId,
            Func<UpgradeNodeSO, int> costResolver,
            CancellationToken cancellationToken = default)
        {
            var spec = _repository.GetSpec(specId);
            if (spec == null)
            {
                LogEx.LogError($"No card spec found for upgrade. ID: {specId}");
                return UiFlowResult<int>.Canceled();
            }

            var availableUpgradeNodes = _upgradeDatabase.GetNextAvailableNodes(spec);

            if (availableUpgradeNodes.Count == 0)
            {
                LogEx.LogError($"No available upgrade nodes. ID: {specId}");
                return UiFlowResult<int>.Canceled();
            }

            if (availableUpgradeNodes.Count > 2)
            {
                LogEx.LogError($"Too many available upgrade nodes for current UI. Count: {availableUpgradeNodes.Count}");
                return UiFlowResult<int>.Canceled();
            }

            var task = _upgradeRequest.Begin(cancellationToken);
            _costResolver = costResolver;
            Open(specId, availableUpgradeNodes);
            var result = await task;
            await _view.PlayCloseAnimationAsync();
            _costResolver = null;
            return result;
        }

        public void Close()
        {
            HandleCloseClicked();
        }

        public void Dispose()
        {
            _view.SelectedNodeChanged -= HandleSelectedNodeChanged;
            _view.CloseClicked -= HandleCloseClicked;
            _view.ConfirmClicked -= HandleUpgradeConfirmClicked;
            _upgradeRequest.Cancel();
            _costResolver = null;
            ClearTarget();
        }


        private void ClearTarget()
        {
            _targetSpecId = -1;
            _availableNodes = null;
        }
        
        private void HandleSelectedNodeChanged(UpgradeNodeSO selectedNode)
        {
            _view.ValidCanUpgrade(ValidUpgrade(selectedNode));
        }

        private void HandleUpgradeConfirmClicked(UpgradeNodeSO selectedNode)
        {
            if (!_upgradeRequest.IsRunning || selectedNode == null || _targetSpecId < 0)
            {
                return;
            }

            if (!ValidUpgrade(selectedNode))
            {
                LogEx.LogError("강화가 가능하지 않지만 View에서 강화가 호출됐습니다.");
                return;
            }
            
            var targetSpec = _repository.GetSpec(_targetSpecId);
            if (targetSpec == null)
            {
                LogEx.LogError($"강화 대상 카드를 찾을 수 없습니다. ID: {_targetSpecId}");
                _upgradeRequest.Cancel();
                ClearTarget();
                return;
            }

            _view.ValidCanUpgrade(false);
            CardevilCore.PlayerStatus.ModifyBaseValue(StatType.Gold, -ResolveCost(selectedNode));
            targetSpec.ApplyUpgradeNodeAndNotify(selectedNode);
            int upgradedSpecId = _targetSpecId;
            CardUpgraded?.Invoke(upgradedSpecId);
            _upgradeRequest.Complete(upgradedSpecId);
            ClearTarget();
        }

        private void HandleCloseClicked()
        {
            bool requestWasRunning = _upgradeRequest.IsRunning;
            ClearTarget();
            _upgradeRequest.Cancel();

            if (!requestWasRunning)
            {
                _view.PlayCloseAnimationAsync().Forget();
            }
        }

        /// <summary>
        /// 강화가 가능한 지 노드를 검증. 일단은 비용만 검증함.
        /// </summary>
        private bool ValidUpgrade(UpgradeNodeSO targetNode)
        {
            if (targetNode == null)
            {
                return false;
            }

            int cost = ResolveCost(targetNode);
            return CardevilCore.PlayerStatus[StatType.Gold] >= cost;
        }

        private int ResolveCost(UpgradeNodeSO node)
        {
            return Math.Max(0, _costResolver?.Invoke(node) ?? node.MarketCost);
        }
    }
}
