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
        
        public PriorityEvent<PlayerHealthChangeArgs> PlayerHealthChangeEvent => _playerHealthChangeEvent;


        public void Clear()
        {
            _playerHealthChangeEvent.Clear();
        }
    }
}