using Cardevil.Utils;
using UnityEngine;

namespace Cardevil.Dungeon.NodePresets
{
    /// <summary>
    /// 암시장 노드 프리셋.
    /// 던전에 미리 배치되지만, 확률에 따라 나타나거나 숨겨집니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BlackMarketNodePreset", menuName = "Cardevil/Dungeon/Node Presets/Black Market", order = 7)]
    [Icon("Assets/Sprites/Dungeon/Icon/Inactive/Black_Market_Inactive.png")]
    public class BlackMarketNodePreset : DungeonNodePreset
    {
        [Header("암시장 설정")]
        [Tooltip("암시장 출현 확률 (0.0 ~ 1.0). 1.0이면 반드시 출현")]
        [SerializeField, Range(0f, 1f)] private float _appearChance = 0.5f;
        
        [Tooltip("true이면 확률 무시하고 반드시 출현")]
        [SerializeField] private bool _alwaysAppear;
        
        /// <summary>
        /// 암시장 출현 확률 (0.0 ~ 1.0)
        /// </summary>
        public float AppearChance => _appearChance;
        
        /// <summary>
        /// 반드시 출현하는 암시장인지 여부
        /// </summary>
        public bool AlwaysAppear => _alwaysAppear;
        
        /// <summary>
        /// 암시장이 나타나야 하는지 확률 체크
        /// </summary>
        public bool ShouldAppear()
        {
            if (_alwaysAppear) return true;
            return Random.value < _appearChance;
        }
        
        public override void OnEnter(DungeonNode node)
        {
            LogEx.Log($"암시장 노드 진입 (ID: {node.NodeId}, 층: {node.Floor}): 암시장이 나타났습니다!");
        }

        public override void OnExit(DungeonNode node, NodeExitInfo exitInfo)
        {
            LogEx.Log($"암시장 노드 탈출 (ID: {node.NodeId}): 암시장 거래를 마쳤습니다.");
        }
    }
}

