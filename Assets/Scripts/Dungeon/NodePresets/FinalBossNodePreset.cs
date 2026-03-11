using Cardevil.Core.Bootstrap;
using Cardevil.Core.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "FinalBossNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Final Boss", order = 5)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Main_Boss_Inactive.png")]
    public class FinalBossNodePreset : DungeonNodePreset
    {

        public override bool RequiresClearToProgress => true;
        
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"최종 보스 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 최종 보스와의 결전을 시작합니다!");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"최종 보스 노드 탈출 (ID: {node.NodeId}): 보스를 격파했습니다!");
            // 다음 던전으로 이동
            DungeonManager dm = CardevilCore.GameFlow.World.Dungeon;
            int nextDungeonId = dm.GetNextDungeonId(node.Dungeon.DungeonId);
            if (nextDungeonId != -1)
            {
                dm.StartDungeonById(nextDungeonId);
            }
        }
    }
}