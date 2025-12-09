using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "MobNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Mob", order = 1)]
    public class MobNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"몬스터 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 몬스터와의 전투가 시작됩니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"몬스터 노드 탈출 (ID: {node.NodeId}): 몬스터를 처치했습니다.");
        }
    }
}