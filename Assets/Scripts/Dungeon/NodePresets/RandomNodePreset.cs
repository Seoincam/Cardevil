using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    [CreateAssetMenu(fileName = "RandomNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Random", order = 3)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Random_Inactive.png")]
    public class RandomNodePreset : DungeonNodePreset
    {
        [Header("랜덤 이벤트 설정")]
        [SerializeField, Tooltip("이벤트 완료 후에만 다음 노드로 이동 가능")]
        private bool requiresClear = true;
        
        /// <summary>
        /// 랜덤 이벤트 노드의 클리어 필요 여부 
        /// </summary>
        public override bool RequiresClearToProgress => requiresClear;
        
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