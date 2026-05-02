using Cardevil.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Core.Upgrade
{
    [CreateAssetMenu(menuName = "NewCards/UpgradeDatabase")]
    public class UpgradeNodeDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<UpgradeNodeSO> allNodes = new();

        private Dictionary<(UpgradePath, int), UpgradeNodeSO> _nodeMap;

        public void Initialize()
        {
            _nodeMap = new Dictionary<(UpgradePath, int), UpgradeNodeSO>();

            foreach (var node in allNodes)
            {
                if (!node) continue;

                var key = (node.Path, node.Stage);
        
                if (_nodeMap.TryGetValue(key, out UpgradeNodeSO value))
                {
                    LogEx.LogError($"중복된 노드 감지! Path: {key.Path}, Stage: {key.Stage}. " +
                                   $"기존: {value.name}, 무시됨: {value.name}");
                    continue;
                }

                _nodeMap.Add(key, node);
            }
        }

        public UpgradeNodeSO GetNode(UpgradePath path, int stage)
        {
            if (_nodeMap == null) Initialize();

            var node = _nodeMap.GetValueOrDefault((path, stage));
            if (!node)
            {
                LogEx.LogError($"강화 노드가 존재하지 않음 (path: {path}, stage: {stage})");
            }

            return node;
        }

        /// <summary>
        /// 가능한 강화 노드를 필터링해서 제공.
        /// </summary>
        public IReadOnlyList<UpgradeNodeSO> GetNextAvailableNodes(CardSpec spec)
        {
            if (!spec.UpgradeNode)
            {
                LogEx.LogError($"현재 강화 노드가 존재하지 않음! ID: {spec.ID}");
                return new List<UpgradeNodeSO>();
            }

            var allNextNodes = spec.UpgradeNode.NextNodes;
            var availableNodes = allNextNodes.Where(node => node.TargetCardType == spec.Type)
                .ToList();
            return availableNodes;
        }
        
#if UNITY_EDITOR
        // 에디터 윈도우에서 자동으로 리스트를 갱신
        public void SyncNodes(List<UpgradeNodeSO> nodes)
        {
            allNodes = nodes;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}