using Cardevil.Events;
using Cardevil.Events.ExecEvents;

namespace Cardevil.Manager
{
    
    /// <summary>
    /// 이벤트 채널을 관리하는 유틸리티 클래스
    /// ExecEventBus 시스템을 사용하므로 각 이벤트는 정적으로 관리됩니다.
    /// </summary>
    /// <remarks>
    /// 이벤트 등록/해제는 다음과 같이 수행합니다:
    /// - 동적 핸들러: ExecEventBus&lt;PlayerHealthChangeArgs&gt;.RegisterDynamic(handler)
    /// - 정적 핸들러: ExecEventBus&lt;PlayerHealthChangeArgs&gt;.RegisterStatic(priority, handler)
    /// - 이벤트 호출: await ExecEventBus&lt;PlayerHealthChangeArgs&gt;.InvokeMerged(args)
    /// 
    /// 이 클래스는 더 이상 인스턴스를 필요로 하지 않으며, 
    /// ExecEventBus를 직접 사용하는 것을 권장합니다.
    /// </remarks>
    public static class EventManager
    {
        /// <summary>
        /// 모든 플레이어 관련 ExecEventBus 핸들러를 정리합니다.
        /// </summary>
        public static void ClearAllPlayerEvents()
        {
            ExecEventBus<PlayerHealthChangeArgs>.ClearAllHandlers();
            ExecEventBus<PlayerShieldChangeArgs>.ClearAllHandlers();
            ExecEventBus<RerollTicketChangeArgs>.ClearAllHandlers();
        }
    }
}

