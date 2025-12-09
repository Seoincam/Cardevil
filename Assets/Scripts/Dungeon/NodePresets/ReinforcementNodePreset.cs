using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "ReinforcementNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Reinforcement", order = 6)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Mob_Inactive.png")]
    public class ReinforcementNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"강화 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 적의 강화가 발생합니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"강화 노드 탈출 (ID: {node.NodeId}): 강화된 적을 물리쳤습니다.");
        }
    }
}