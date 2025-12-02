using System;
using UnityEngine;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 노드의 실행 로직을 담당하는 클래스.
    /// Strategy Pattern을 활용하여 각 노드 타입별 동작을 정의합니다.
    /// </summary>
    [Serializable]
    public abstract class DungeonNodePreset : ScriptableObject
    {
        /// <summary>
        /// 이 노드가 던전에 등장하는 횟수 (향후 사용 예정)
        /// </summary>
        [Tooltip("던전 생성 시 이 노드가 등장할 횟수")]
        public int count = 1;

        /// <summary>
        /// 노드에 진입했을 때 실행되는 로직
        /// </summary>
        public abstract void OnEnter();
        
        /// <summary>
        /// 노드에서 나갈 때 실행되는 로직 (선택적)
        /// </summary>
        public virtual void OnExit()
        {
            
        }
    }
}