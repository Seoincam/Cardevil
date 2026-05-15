using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Utils;
using Cardevil.Gameplay;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Cardevil.Card.InWorld.Shop.Upgrade
{
    public class CardUpgradePresenter
    {
        /// <summary>
        /// 카드가 업그레이드됐을 때 발행되는 이벤트. 해당 카드의 ID를 인자로 함.
        /// </summary>
        public event Action<int> CardUpgraded;
        
        
        private readonly CardRepository _repository;
        private readonly UpgradeNodeDatabaseSO _upgradeDatabase;
        private readonly CardUpgradeView _view;
        
        private int _targetSpecId;
        private IReadOnlyList<UpgradeNodeSO> _availableNodes;


        public struct UpgradeData
        { 
            public readonly UpgradeNodeSO UpgradeNode;
            public readonly CardVisualInput VisualInput;

            public UpgradeData(UpgradeNodeSO upgradeNode, CardVisualInput visualInput)
            {
                UpgradeNode = upgradeNode;
                VisualInput = visualInput;
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
            var availableUpgradeNodes = _upgradeDatabase.GetNextAvailableNodes(spec);

            if (availableUpgradeNodes.Count == 0)
            {
                LogEx.LogError($"가능한 강화 노드가 존재하지 않음. ID: {specId}");
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
            
            
            var originalState = _repository.GetState(specId);
            var originalVisual = CardVisualInput.From(originalState);

            switch (_availableNodes.Count)
            {
                case 1 :
                    var nextSpec = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[0]);
                    var nextVisual = CardVisualInput.From(nextSpec);
                    var data = new UpgradeData(availableNodes[0], nextVisual);
                    
                    _view.Create(originalVisual, data);
                    break;
                
                case 2: 
                    var nextSpec1 = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[0]);
                    var nextVisual1 = CardVisualInput.From(nextSpec1.State);
                    var data1 = new UpgradeData(availableNodes[0], nextVisual1);

                    var nextSpec2 = _repository.GetDeepClonedSpec(specId)
                        .ApplyUpgradeNode(availableNodes[1]);
                    var nextVisual2 = CardVisualInput.From(nextSpec2.State);
                    var data2 = new UpgradeData(availableNodes[1], nextVisual2);

                
                    _view.Create(originalVisual, data1, data2);
                    break;
            }
            
            _view.PlayOpenAnimationAsync().Forget();
        }

        public void Close()
        {
            HandleCloseClicked();
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
            if (!ValidUpgrade(selectedNode))
            {
                LogEx.LogError("강화가 가능하지 않지만 View에서 강화가 호출됐습니다.");
                return;
            }
            
            var targetSpec = _repository.GetSpec(_targetSpecId);
            targetSpec.ApplyUpgradeNode(selectedNode);
            CardUpgraded?.Invoke(_targetSpecId);
            
            HandleCloseClicked();
        }

        private void HandleCloseClicked()
        {
            ClearTarget();
            _view.PlayCloseAnimationAsync().Forget();
        }

        /// <summary>
        /// 강화가 가능한 지 노드를 검증. 일단은 비용만 검증함.
        /// </summary>
        private bool ValidUpgrade(UpgradeNodeSO targetNode)
        {
            int cost = targetNode.MarketCost;
            return CardevilCore.PlayerStatus[StatType.Gold] >= cost;
        }
    }
}