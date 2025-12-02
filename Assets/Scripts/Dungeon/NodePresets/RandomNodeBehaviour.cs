using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "HealNodeBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Heal Node Behaviour", order = 1)]
    public class RandomNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Entering Random Node (ID: {node.NodeId}, Floor: {node.Floor}): Player encounters a random event.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Exiting Random Node (ID: {node.NodeId}): Player leaves the random event.");
        }
    }
}