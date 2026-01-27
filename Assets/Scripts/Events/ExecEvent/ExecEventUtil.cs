using Cardevil.Cards.Evaluations;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cardevil.Events.ExecEvents
{
    public delegate void ExecEventHandler<TEvent>(ExecQueue<TEvent> queue, TEvent eventArgs) where TEvent : ExecEventArgs<TEvent>, new();
    public delegate UniTask ExecAction<TEvent>(TEvent eventArgs, CancellationToken cancellationToken) where TEvent : ExecEventArgs<TEvent>, new();

    /// <summary>
    /// 우선순위 실행 이벤트 유틸리티 클래스
    /// </summary>
    public static class ExecEventUtil
    {
        public static IReadOnlyList<Type> EventTypes;
        public static IReadOnlyList<Type> EventBusTypes;
        public static IReadOnlyList<Type> StaticEventBusTypes;
        
        private static bool _initialized;

        #if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; private set; }

        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            PlayModeState = state;
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClearBus();
            }
        }
        #endif
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            LogEx.Log("Initializing ExecEventUtil");
            EventTypes = ReflectionUtil.GetTypes(typeof(ExecEventArgs<>));
            EventBusTypes = InitializeAllBus();
            StaticEventBusTypes = InitializeAllStaticBus();
            _initialized = true;
        }

        private static List<Type> InitializeAllBus()
        {
            List<Type> busTypes = new List<Type>();
            var busType = typeof(ExecDynamicEventBus<>);
            foreach (var eventType in EventTypes)
            {
                var genericBusType = busType.MakeGenericType(eventType);
                busTypes.Add(genericBusType);
                LogEx.Log($"Initialized ExecEventBus for {eventType.Name}");
            }
            return busTypes;
        }
        
        private static List<Type> InitializeAllStaticBus()
        {
            List<Type> staticBusTypes = new List<Type>();
            var staticBusType = typeof(ExecStaticEventBus<>);
            foreach (var eventType in EventTypes)
            {
                var genericStaticBusType = staticBusType.MakeGenericType(eventType);
                staticBusTypes.Add(genericStaticBusType);
                LogEx.Log($"Initialized ExecStaticEventBus for {eventType.Name}");
            }
            return staticBusTypes;
        }
        
        public static void ClearBus()
        {
            LogEx.Log("Clearing all ExecEventBus handlers");
            foreach (var busType in EventBusTypes)
            {
                var clearMethod = busType.GetMethod("ClearHandlers", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(null, null);
                }
                else
                {
                    LogEx.LogWarning($"ClearHandlers method not found in {busType.Name}");
                }
            }
            LogEx.Log("Clearing all ExecStaticEventBus handlers");
            foreach (var staticBusType in StaticEventBusTypes)
            {
                var clearMethod = staticBusType.GetMethod("ClearHandlers", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(null, null);
                }
                else
                {
                    LogEx.LogWarning($"ClearHandlers method not found in {staticBusType.Name}");
                }
            }
        }

        /// <summary>
        /// EventArgs의 타입 이름을 네임스페이스를 제거해 반환합니다.
        /// </summary>
        public static string GetEventArgsName(Type eventType)
        {
            const string prefix = "Cardevil.Events.";
            string name = eventType.FullName ?? eventType.Name;
            return name.StartsWith(prefix) ? name[prefix.Length..] : name;
        }

    }
}