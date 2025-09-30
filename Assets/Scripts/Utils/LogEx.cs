using System;
using System.Diagnostics;

namespace Cardevil.Utils
{
    public interface ILogExSupport
    {
        /// <summary>
        /// 해당 로그 레벨 미만은 출력하지 않습니다.
        /// </summary>
        /// <example>
        /// LogLevel.Warning로 설정하면 Warning과 Error만 출력되고 Info는 출력되지 않습니다.
        /// </example>
        public LogEx.LogLevel LogLevel { get; }
    }
    public static class LogEx
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            NoLog,
        }

        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(object obj, LogLevel level = LogLevel.Info, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", level, depth + 1);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(string message, LogLevel level = LogLevel.Info, int depth = 1)
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(depth);
            var classType = frame.GetMethod().DeclaringType;
            var isLoggingClass = typeof(ILogExSupport).IsAssignableFrom(classType);
            if (isLoggingClass)
            {
                if (Activator.CreateInstance(classType!) is ILogExSupport instance && level < instance.LogLevel)
                {
                    return;
                }
            }
            var className = classType?.Name ?? "UnknownClass";
            var method = frame.GetMethod();
            string logMessage = message.StartsWith('[') ? message : $"[{className} :: {method.Name}] {message}";
            switch (level)
            {
                case LogLevel.NoLog:
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
            }
        }

        [Conditional("UNITY_EDITOR")]
        public static void Log(Exception ex, int depth = 1)
        {
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(depth);
            var classType = frame.GetMethod().DeclaringType;
            var isLoggingClass = typeof(ILogExSupport).IsAssignableFrom(classType);
            if (isLoggingClass)
            {
                if (Activator.CreateInstance(classType!) is ILogExSupport instance && LogLevel.Error < instance.LogLevel)
                {
                    return;
                }
            }
            var className = classType?.Name ?? "UnknownClass";
            var method = frame.GetMethod();
            string logMessage = $"[{className} :: {method.Name}] Exception occurred - {ex.Message}\n{ex.StackTrace}";
            UnityEngine.Debug.LogError(logMessage);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object obj, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", LogLevel.Warning, depth + 1);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(string message, int depth = 1)
        {
            Log(message, LogLevel.Warning, depth + 1);
        }
        
        [Conditional("UNITY_EDITOR")] 
        public static void LogError(object obj, int depth = 1)
        {
            Log(obj?.ToString() ?? "null", LogLevel.Error, depth + 1);
        }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(string message, int depth = 1)
        {
            Log(message, LogLevel.Error, depth + 1);
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void LogError(Exception ex, int depth = 1)
        { 
            Log(ex, depth + 1);
        }
        
        public static bool IsLogLevelEnabled(ILogExSupport obj, LogLevel level)
        {
            if (obj == null) return true; // null이면 무조건 출력
            return level >= obj.LogLevel;
        }
        
    }
}