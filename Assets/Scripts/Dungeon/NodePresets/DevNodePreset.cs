using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "DevNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Dev", order = 10)]
    public class DevNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"Dev 노드 진입 (ID: {node.NodeId}, 층: {node.Floor})");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"Dev 노드 탈출 (ID: {node.NodeId})");
        }
    }
}