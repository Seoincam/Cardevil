using Cardevil.Core;
using Cardevil.Events;

namespace Cardevil.Manager
{
    
    /// <summary>
    /// 이벤트 채널을 관리하는 클래스
    /// </summary>
    public class EventManager : IClearable
    {
        private readonly PriorityEvent<PlayerHealthChangeArgs> _playerHealthChangeEvent = new ();
        private readonly PriorityEvent<RemainingCardChangeArgs> _remainingCardChangeEvent = new ();
        private readonly PriorityEvent<PlayerShieldChangeArgs> _playerShieldChangeEvent = new ();
        
        /// <summary>
        /// 플레이어의 체력 변화 이벤트
        /// </summary>
        /// <code>
        /// 우선순위
        /// -1 :
        /// 0 : 기본 이벤트
        /// 10 : UI 업데이트 이벤트
        /// </code>
        public PriorityEvent<PlayerHealthChangeArgs> PlayerHealthChangeEvent => _playerHealthChangeEvent;

        /// <summary>
        /// 남은 카드 수 변화 이벤트
        /// </summary>
        /// <code>
        /// 우선순위
        /// 0 : UI 업데이트 이벤트  
        /// </code>
        public PriorityEvent<RemainingCardChangeArgs> RemainingCardChangeEvent => _remainingCardChangeEvent;
        
        /// <summary>
        /// 플레이어의 방어막 변화 이벤트
        /// </summary>
        /// <code>
        /// 우선순위
        /// 미정
        /// </code>
        public PriorityEvent<PlayerShieldChangeArgs> PlayerShieldChangeEvent => _playerShieldChangeEvent;

        public void Clear()
        {
            _playerHealthChangeEvent.Clear();
            _remainingCardChangeEvent.Clear();
            _playerShieldChangeEvent.Clear();
        }
    }
}