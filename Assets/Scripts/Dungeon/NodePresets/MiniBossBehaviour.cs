using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "MiniBossBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Mini Boss Behaviour", order = 1)]
    public class MiniBossBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Mini Boss Node Entered (ID: {node.NodeId}, Floor: {node.Floor})");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Mini Boss Node Exited (ID: {node.NodeId})");
        }
    }
}