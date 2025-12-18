using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "MiniBossNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Mini Boss", order = 4)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Middle_Boss_Inactive.png")]
    public class MiniBossNodePreset : DungeonNodePreset
    {
        public override bool RequiresClearToProgress => true;
        
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"중간 보스 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 중간 보스와의 전투가 시작됩니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"중간 보스 노드 탈출 (ID: {node.NodeId}): 중간 보스를 물리쳤습니다.");
        }
    }
}