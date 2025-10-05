using Cardevil.DebugConsole.Commands;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cardevil.DebugConsole
{
    /// <summary>
    /// 디버그 콘솔의 메인 클래스입니다.
    /// 콘솔의 Model과 Controller 역할을 합니다.
    /// </summary>
    public class Console
    {
        private static Console _console;
        public static Console Instance => _console ??= new Console();
        
        
        private IConsoleWindow _window;
        private LogLevel _logPrintLevel = LogLevel.Warning;
        
        public LogLevel LogPrintLevel
        {
            get => _logPrintLevel;
            set => _logPrintLevel = value;
        }
        
        public bool IsWindowOpen => _window?.IsOpen ?? false;
#if !DO_NOT_USE_DEBUG_CONSOLE
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_console != null) return;
            _console = new Console();
            
            Application.logMessageReceived += _console.OnLogReceived;
        }

        ~Console()
        {
            Application.logMessageReceived -= OnLogReceived;
        }
#endif
        /// <summary>
        /// 콘솔 윈도우를 등록합니다.
        /// 이미 등록된 윈도우가 있다면 교체합니다.
        /// </summary>
        /// <param name="window"></param>
        public void RegisterWindow(IConsoleWindow window)
        {
            if (_window != null)
            {
                LogEx.LogWarning("콘솔 윈도우가 이미 등록되어 있습니다. 기존 윈도우를 대체합니다.");
                _window.Close();
                if (_window is MonoBehaviour behaviour)
                {
                    UnityEngine.Object.Destroy(behaviour);
                }
            }
            _window = window;
        }
        
        /// <summary>
        /// 콘솔 윈도우 등록을 해제합니다.
        /// </summary>
        /// <param name="window">해제할 윈도우. null 이거나 현재 등록된 윈도우와 같아야 해제됩니다.</param>
        public void UnregisterWindow(IConsoleWindow window)
        {
            if (_window == null) return;
            
            if (window == null || _window == window)
            {
                _window.Close();
                if (_window is MonoBehaviour behaviour)
                {
                    UnityEngine.Object.Destroy(behaviour);
                }
                _window = null;
            }
        }
        
        /// <summary>
        /// 자동 완성 제안을 가져옵니다.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="suggestions"></param>
        public void GetAutoCompleteSuggestions(string[] input, ref List<string> suggestions)
        {
            suggestions.Clear();
            
        }
        
        /// <summary>
        /// 로그가 수신되었을 때 호출됩니다.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void OnLogReceived(string condition, string stackTrace, LogType type)
        {
            if(_window == null) return;
            if ((int)type > (int)_logPrintLevel) return;
            _window.Print(type, condition);
        }
        
        
        /// <summary>
        /// 입력된 명령어를 실행합니다.
        /// 명령어가 실행되면 true, 명령어가 없거나 빈 입력이면 false를 반환합니다.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool ExecuteCommand(string input)
        {
    
#if !DO_NOT_USE_DEBUG_CONSOLE
            if (_window == null) return false;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0];
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (CommandLibrary.TryGetCommand(commandName, out var command))
            {
                try
                {
                    LogEx.Log($"Executing command: {commandName} with args: {string.Join(", ", args)}");
                    command.Execute(args);
                }
                catch (Exception ex)
                {
                    Message(MessageType.Error, $"Error executing command '{commandName}': {ex.Message}");
                }
                return true;
            }
            else
            {
                Message(MessageType.Error, $"Unknown command: '{commandName}'. Type 'help' for a list of commands.");
                return false;
            }
#else
            return false;
#endif
        }
        
        public static void Print(string message)
        {
            Instance.PrintInternal(LogType.Log, message);
        }
        public static void Print(LogType type, string message)
        {
            Instance.PrintInternal(type, message);
        }
        
        public static void Message(string message)
        {
            Instance.MessageInternal(message);
        }

        public static void Message(MessageType type, string message)
        {
            Instance.MessageInternal(type, message);
        }
        
        private void MessageInternal(string message)
        {
            _window?.Message(message);
        }
        private void MessageInternal(MessageType type, string message)
        {
            _window?.Message(type, message);
        }
        private void PrintInternal(LogType type, string message)
        {
            _window?.Print(type, message);
        }
        
        
        [Preserve, ConsoleCommand("echo", "Prints the input arguments back to the console.")]
        private static void EchoCommand(string[] args)
        {
            string message = string.Join(" ", args);
            Instance.PrintInternal(LogType.Log, message);
        }

        [Preserve, ConsoleCommand("help", "Lists all available commands.")]
        private static void HelpCommand(string[] args)
        {
            var commands = CommandLibrary.GetAllCommands();
            Instance.PrintInternal(LogType.Log, "Available Commands:");
            foreach (var command in commands)
            {
                Instance.PrintInternal(LogType.Log, $"- {command.Command}: {command.Description}");
            }
        }
        
        /// <summary>
        /// 로그 출력 레벨을 설정합니다.
        /// 수동 커맨드 등록 예제입니다.
        /// <see cref="CommandLibrary.setLogLevelCommand"/> 에서 호출됩니다.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLogLevel(LogLevel level)
        {
            Instance.LogPrintLevel = level;
        }
        
    }
}