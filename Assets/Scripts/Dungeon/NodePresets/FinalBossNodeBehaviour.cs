using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "FinalBossNodeBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Final Boss Node Behaviour", order = 1)]
    public class FinalBossNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Entering Final Boss Node (ID: {node.NodeId}, Floor: {node.Floor}): Fight the final boss.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Exiting Final Boss Node (ID: {node.NodeId}).");
        }
    }
}