using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "MobNodeBehaviour", menuName = "Cardevil/Dungeon/Node Presets/Mob Node Behaviour", order = 1)]
    public class MobNodeBehaviour : DungeonNodeBehaviour
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Entering Mob Node (ID: {node.NodeId}, Floor: {node.Floor})");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Exiting Mob Node (ID: {node.NodeId})");
        }
    }
}