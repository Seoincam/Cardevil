using Cardevil.Dungeon.Core;
using System;
using UnityEngine;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 노드의 실행 로직을 담당하는 클래스.
    /// 현재로썬 기능이 없음
    /// </summary
    [Serializable]
    public abstract class DungeonNodeBehaviour : ScriptableObject
    {
        public int count = 1;
        
        
        [SerializeField] protected float blackmarketWeight = 1f;
        public float BlackmarketWeight => blackmarketWeight;

        public abstract void OnEnter();
        public virtual void OnExit(NodeExitInfo info)
        {
            
        }
    }
}