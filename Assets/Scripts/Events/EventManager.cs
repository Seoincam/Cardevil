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
        public PriorityEvent<RemainingCardChangeArgs> RemainingCardChangeEvent => _remainingCardChangeEvent;



        public void Clear()
        {
            _playerHealthChangeEvent.Clear();
            _remainingCardChangeEvent.Clear();
        }
    }
}