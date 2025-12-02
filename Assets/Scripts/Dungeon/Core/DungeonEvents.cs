using System;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 던전 관련 이벤트를 관리하는 클래스
    /// </summary>
    public class DungeonEvents
    {
        /// <summary>
        /// 노드에 진입했을 때 발생하는 이벤트
        /// </summary>
        public event Action<DungeonNode> OnNodeEntered;
        
        /// <summary>
        /// 노드에서 나갈 때 발생하는 이벤트
        /// </summary>
        public event Action<DungeonNode> OnNodeExited;
        
        /// <summary>
        /// 던전을 완료했을 때 발생하는 이벤트
        /// </summary>
        public event Action<Dungeon> OnDungeonCompleted;
        
        /// <summary>
        /// 던전이 초기화되었을 때 발생하는 이벤트
        /// </summary>
        public event Action<Dungeon> OnDungeonInitialized;
        
        /// <summary>
        /// 노드 진입 이벤트 호출
        /// </summary>
        public void RaiseNodeEntered(DungeonNode node)
        {
            OnNodeEntered?.Invoke(node);
        }
        
        /// <summary>
        /// 노드 이탈 이벤트 호출
        /// </summary>
        public void RaiseNodeExited(DungeonNode node)
        {
            OnNodeExited?.Invoke(node);
        }
        
        /// <summary>
        /// 던전 완료 이벤트 호출
        /// </summary>
        public void RaiseDungeonCompleted(Dungeon dungeon)
        {
            OnDungeonCompleted?.Invoke(dungeon);
        }
        
        /// <summary>
        /// 던전 초기화 이벤트 호출
        /// </summary>
        public void RaiseDungeonInitialized(Dungeon dungeon)
        {
            OnDungeonInitialized?.Invoke(dungeon);
        }
    }
}

