using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "HealNodeBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Heal Node Behaviour", order = 1)]
    public class HealNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Entering Heal Node (ID: {node.NodeId}, Floor: {node.Floor}): Player is healed.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Exiting Heal Node (ID: {node.NodeId}).");
        }
    }
}