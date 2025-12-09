﻿using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "HealNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Heal", order = 2)]
    public class HealNodePreset : DungeonNodePreset
    {
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"회복 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 플레이어가 회복합니다.");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"회복 노드 탈출 (ID: {node.NodeId}).");
        }
    }
}