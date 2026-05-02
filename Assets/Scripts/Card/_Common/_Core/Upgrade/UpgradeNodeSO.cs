using UnityEngine;

namespace Cardevil.Card.Common.Core.Upgrade
{
    public class UpgradeNodeSO : ScriptableObject
    {
        [SerializeField] private UpgradeNode upgradeNode;
        
        public UpgradeNode UpgradeNode => upgradeNode;
    }
}