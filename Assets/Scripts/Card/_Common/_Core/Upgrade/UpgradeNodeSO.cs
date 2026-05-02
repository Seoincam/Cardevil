using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Common.Core.Upgrade
{
    [CreateAssetMenu(menuName = "NewCards/UpgradeNode")]
    public class UpgradeNodeSO : ScriptableObject
    {
        [Header("Nodes")]
        [SerializeField] private List<UpgradeNodeSO> nextNodes = new();
        
        [field: Header("Current")]
        [SerializeField] public UpgradePath path;
        [SerializeField] public int stage;
        
        [Header("Constraints")]
        [SerializeField] private CardType targetCardType;
        
        [Header("Actions")]
        [SerializeField] private UpgradeApplyType upgradeType;
        [SerializeReference] private List<ISpecElement> elements = new();
        
        [Header("Cost")]
        [SerializeField] private int blackMarketCost;
        [SerializeField] private int marketCost;

        public enum UpgradeApplyType
        {
            Add,
            Override
        }
        
        public IReadOnlyList<UpgradeNodeSO> NextNodes => nextNodes;
        
        public UpgradePath Path => path;
        public int Stage => stage;
        public CardType TargetCardType => targetCardType;
        public UpgradeApplyType UpgradeType => upgradeType;
        public IReadOnlyList<ISpecElement> Elements => elements;
        
        public int BlackMarketCost => blackMarketCost;
        public int MarketCost => marketCost;
        
#if UNITY_EDITOR
        [HideInInspector] public Vector2 nodePosition = new(100, 100);
        
        // 에디터에서 선을 연결/해제할 때 사용할 헬퍼 메서드
        public void AddNextNode(UpgradeNodeSO node) 
        { 
            if (!nextNodes.Contains(node)) nextNodes.Add(node); 
        }
        public void RemoveNextNode(UpgradeNodeSO node) 
        { 
            nextNodes.Remove(node); 
        }
#endif
    }
}