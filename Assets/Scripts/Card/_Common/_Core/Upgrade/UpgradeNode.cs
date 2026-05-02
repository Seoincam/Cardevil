using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Common.Core.Upgrade
{
    [Serializable]
    public class UpgradeNode
    {
        [Header("Nodes")]
        [SerializeField] private List<UpgradeNode> nextNodes = new();
        
        [field: Header("Current")]
        [field: SerializeField] public UpgradePath Path { get; private set; }
        [field: SerializeField] public int Stage { get; private set; }
        
        [Header("Actions")]
        [SerializeField] private UpgradeType upgradeType;
        [SerializeReference] private List<ISpecElement> elements = new();

        private enum UpgradeType
        {
            Add,
            Override
        }
        
        public IReadOnlyList<UpgradeNode> NextNodes => nextNodes;
    }
}