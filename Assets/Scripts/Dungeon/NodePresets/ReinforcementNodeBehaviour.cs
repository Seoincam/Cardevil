using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "ReinforcementNodeBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Reinforcement Node Behaviour", order = 1)]
    public class ReinforcementNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Reinforcement Node Entered (ID: {node.NodeId}, Floor: {node.Floor})");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Reinforcement Node Exited (ID: {node.NodeId})");
        }
    }
}