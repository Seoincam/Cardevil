using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Utils;
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
        
        
        public CardUpgradePresenter(CardRepository repository, UpgradeNodeDatabaseSO upgradeDatabase, CardUpgradeView view)
        {
            _repository = repository;
            _upgradeDatabase = upgradeDatabase;
            _view = view;
            
            _view.UpgradeRequested += HandleUpgradeRequested;
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
            
            // 현재 상태
            var originalState = _repository.GetState(specId);
            var originalVisual = CardVisualInput.From(originalState);


            switch (_availableNodes.Count)
            {
                case 1 :
                    var nextSpec = _repository
            }
            // 가능한 다음 강화 적용한 상태
            if (availableNodes.Count == 1)
            {
                var nextSpec = _repository.GetDeepClonedSpec(specId)
                    .ApplyUpgradeNode(availableNodes[0]);
                var nextVisual = CardVisualInput.From(nextSpec.State);
                
                _view.Create(originalVisual, nextVisual);
            }
            else if (availableNodes.Count == 2)
            {
                var nextSpec1 = _repository.GetDeepClonedSpec(specId)
                    .ApplyUpgradeNode(availableNodes[0]);
                var nextSpec2 = _repository.GetDeepClonedSpec(specId)
                    .ApplyUpgradeNode(availableNodes[1]);
                
                var nextVisual1 = CardVisualInput.From(nextSpec1.State);
                var nextVisual2 = CardVisualInput.From(nextSpec2.State);
                
                _view.Create(originalVisual, nextVisual1, nextVisual2);
            }
            
            _view.PlayOpenAnimationAsync().Forget();
        }

        public void Clear()
        {
            _targetSpecId = -1;
            _availableNodes = null;

            if (_view)
            {
                _view.UpgradeRequested -= HandleUpgradeRequested;
            }
        }

        private void HandleUpgradeConfirmButtonClicked()
        {
            
        }

        private void HandleUpgradeRequested(int index)
        {
            var selectedUpgradeNode = _availableNodes[index];
            _repository.GetSpec(_targetSpecId).ApplyUpgradeNode(selectedUpgradeNode);
            
            Clear();
            _view.FadeOut().ContinueWith(() => CardUpgraded?.Invoke());
        }
        
        private static void Create
    }
}