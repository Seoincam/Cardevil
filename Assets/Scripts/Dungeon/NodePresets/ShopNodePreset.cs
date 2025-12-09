using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "ShopNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Shop", order = 8)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Shop_Inactive.png")]
    public class ShopNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"상점 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 상점을 발견했습니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"상점 노드 탈출 (ID: {node.NodeId}): 상점을 떠납니다.");
        }
    }
}

