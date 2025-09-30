using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
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
        const string LoggerName = "LogEx";
        const string LoggerCs = "LogEx.cs";
        
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
        
        
    #if UNITY_EDITOR
    [OnOpenAsset(0)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj == null || obj.name != LoggerName)
        {
            return false;
        }
        string stackTrace = GetStackTrace();
        if(!string.IsNullOrEmpty(stackTrace)) // can customize the label to be added here; the original code is confusing and does not need to be modified, you need to locate it yourself;
        {
            Match matches = Regex.Match(stackTrace, @"\(at(.+)\)", RegexOptions.IgnoreCase);
            string pathline = "";
            while (matches.Success)
            {
                pathline = matches.Groups[1].Value;
                if (!pathline.Contains(LoggerCs)) break;
                matches = matches.NextMatch();
            }

            if (matches.Success)
            {
                pathline = matches.Groups[1].Value;
                pathline = pathline.Replace(" ", "");

                int split_index = pathline.LastIndexOf(":");
                string path = pathline.Substring(0, split_index);
                line = Convert.ToInt32(pathline.Substring(split_index + 1));
                string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                fullpath = fullpath + path;
                string strPath = fullpath.Replace('/', '\\');
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(strPath, line);
            }
            return true;
            
        }
        return false;
    }
    #endif
    
    /// <summary>
    /// 유니티 콘솔창에서 스택트레이스를 가져옵니다.
    /// https://www.programmersought.com/article/3933462942/
    /// </summary>
    /// <returns></returns>
    private static string GetStackTrace()
	{
		 // Find the assembly of UnityEditor.EditorWindow
		var assembly_unity_editor = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
		if (assembly_unity_editor == null) return null;

		 // Find the class UnityEditor.ConsoleWindow
		var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
		if (type_console_window == null) return null;
		 // Find the member ms_ConsoleWindow in UnityEditor.ConsoleWindow
		var field_console_window = type_console_window.GetField("ms_ConsoleWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		if (field_console_window == null) return null;
		 // Get the value of ms_ConsoleWindow
		var instance_console_window = field_console_window.GetValue(null);
		if (instance_console_window == null) return null;

		 // If the focus window of the console window, get the stacktrace
		if ((object)UnityEditor.EditorWindow.focusedWindow == instance_console_window)
		{
			 // Get the class ListViewState through the assembly
			var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
			if (type_list_view_state == null) return null;

			 // Find the member m_ListView in the class UnityEditor.ConsoleWindow
			var field_list_view = type_console_window.GetField("m_ListView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (field_list_view == null) return null;

			 // Get the value of m_ListView
			var value_list_view = field_list_view.GetValue(instance_console_window);
			if (value_list_view == null) return null;

			 // Find the member m_ActiveText in the class UnityEditor.ConsoleWindow
			var field_active_text = type_console_window.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (field_active_text == null) return null;

			 // Get the value of m_ActiveText, is the stacktrace we need
			string value_active_text = field_active_text.GetValue(instance_console_window).ToString();
			return value_active_text;
		}

		return null;
	}
        
        
        public static bool IsLogLevelEnabled(ILogExSupport obj, LogLevel level)
        {
            if (obj == null) return true; // null이면 무조건 출력
            return level >= obj.LogLevel;
        }
        
    }
}