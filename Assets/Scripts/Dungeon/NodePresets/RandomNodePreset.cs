using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "RandomNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Random", order = 3)]
    public class RandomNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"랜덤 이벤트 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 플레이어가 랜덤 이벤트를 만났습니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"랜덤 이벤트 노드 탈출 (ID: {node.NodeId}): 플레이어가 이벤트를 마쳤습니다.");
        }
    }
}