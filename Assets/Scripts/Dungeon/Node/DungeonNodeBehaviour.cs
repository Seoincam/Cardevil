using System;
using UnityEngine;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 노드의 실행 로직을 담당하는 클래스.
    /// DungeonNode 진입/퇴장 시 호출되는 메서드를 정의합니다.
    /// </summary>
    [Serializable]
    public abstract class DungeonNodeBehaviour : ScriptableObject
    {
        public int count = 1;
        
        
        [SerializeField] protected float blackmarketWeight = 1f;
        public float BlackmarketWeight => blackmarketWeight;

        /// <summary>
        /// 노드에 진입할 때 호출됩니다.
        /// </summary>
        /// <param name="node">진입한 노드</param>
        public abstract void OnEnter(DungeonNode node);
        
        /// <summary>
        /// 노드에서 나갈 때 호출됩니다.
        /// </summary>
        /// <param name="node">나가는 노드</param>
        /// <param name="info">퇴장 정보</param>
        public virtual void OnExit(DungeonNode node, NodeExitInfo info)
        {
            
        }
    }
}