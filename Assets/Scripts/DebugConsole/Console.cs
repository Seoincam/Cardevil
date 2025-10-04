using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cardevil.DebugConsole
{
    public interface IConsoleCommand
    {
        string Command { get; }
        string Description { get; }
        
        
        string Signature => Command + " " + (Description ?? "");
        void Execute(string[] args);
        
        void AutoComplete(string[] args, ref List<string> suggestions) { }
    }

    public interface IConsoleWindow
    {
        void Open();
        void Close();
        void Print(string message);
        void Print(LogType type, string message);
        void ClearHistory();
        void Toggle();
        bool IsOpen { get; }
    }
    
    /// <summary>
    /// 하위 명령어를 가질 수 있는 트리 구조의 콘솔 명령어입니다.
    /// </summary>
    public class TreeConsoleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        
        private List<IConsoleCommand> _subCommands = new List<IConsoleCommand>();
        public TreeConsoleCommand(string command, string description = "")
        {
            Command = command;
            Description = description;
        }
        public TreeConsoleCommand AddSubCommand(IConsoleCommand command)
        {
            _subCommands.Add(command);
            return this;
        }
        public string Signature
        {
            get
            {
                string subCommands = string.Join("|", _subCommands.ConvertAll(c => c.Command));
                return $"{Command} ({subCommands}) - {Description}";
            }
        }

        public void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Print($"Usage: {Signature}");
                return;
            }

            string subCommandName = args[0];
            var subCommand = _subCommands.Find(c => c.Command.Equals(subCommandName, StringComparison.OrdinalIgnoreCase));
            if (subCommand != null)
            {
                subCommand.Execute(args.Length > 1 ? args[1..] : Array.Empty<string>());
            }
            else
            {
                Console.Print($"Unknown sub-command '{subCommandName}'. Available sub-commands: {string.Join(", ", _subCommands.ConvertAll(c => c.Command))}");
            }
        }
    }
    
    /// <summary>
    /// 리플렉션으로 등록되는 콘솔 명령어입니다.
    /// </summary>
    public class ReflectionConsoleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly System.Reflection.MethodInfo _method;

        public ReflectionConsoleCommand(string command, string description, System.Reflection.MethodInfo method)
        {
            Command = command;
            Description = description;
            _method = method;
        }

        public void Execute(string[] args)
        {
            _method.Invoke(null, new object[] { args });
        }
    }
    
 
    /// <summary>
    /// 메서드에 적용하여 콘솔 명령어로 등록합니다.
    /// </summary>
    [Preserve]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string Command { get; }
        public string Description { get; }

        public ConsoleCommandAttribute(string command, string description = "")
        {
            Command = command;
            Description = description;
        }
    }
    

    /// <summary>
    /// 등록된 콘솔 명령어들을 관리합니다.
    /// </summary>
    [Preserve]
    public static class CommandLibrary
    {
        /// <summary>
        /// 명령어 이름을 키로, IConsoleCommand 객체를 값으로 가지는 사전
        /// 순차 탐색을 위해 SortedDictionary 사용
        /// </summary>
        private static readonly SortedDictionary<string, IConsoleCommand> _commands = new SortedDictionary<string, IConsoleCommand>();
        
        /// <summary>
        /// 콘솔 명령어를 등록합니다.
        /// </summary>
        /// <param name="command"></param>
        public static void RegisterCommand(IConsoleCommand command)
        {
            if (_commands.ContainsKey(command.Command))
            {
                LogEx.LogWarning($"Command '{command.Command}' is already registered. Overwriting.");
            }
            else
            {
                LogEx.Log($"Registered command: {command.Command}");
            }
            _commands[command.Command] = command;
        }

        /// <summary>
        /// 콘솔 명령어를 이름으로 검색합니다.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool TryGetCommand(string commandName, out IConsoleCommand command)
        {
            return _commands.TryGetValue(commandName, out command);
        }

        /// <summary>
        /// 등록된 모든 콘솔 명령어를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IConsoleCommand> GetAllCommands()
        {
            return _commands.Values;
        }
        
        static SimpleCommand<int> setLogLevelCommand = new SimpleCommand<int>(
            "setLogLevel",
            "어떤 유니티 로그를 콘솔에 출력할 지 결정합니다. Usage: setLogLevel <0-4> (0: None, 1: Exception, 2: Error, 3: Warning, 4: Info)",
            (level) =>
            {
                if (level < 0)
                {
                    Console.Print("Invalid log level. Minimum is 0 (None).");
                    return;
                }
                if(level > (int)LogLevel.Info)
                {
                    level = (int)LogLevel.Info;
                }
                Console.SetLogLevel((LogLevel)level);
                Console.Print($"Log level set to {(LogLevel)level}");
            });
        
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            /*
             * 수동 등록
             */
            RegisterCommand(setLogLevelCommand);
            
            
            /*
             * ConsoleCommandAttribute 를 전부 찾아서 등록
             */
            var commandTypes = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in commandTypes)
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                    {
                        var attrs = method.GetCustomAttributes(typeof(ConsoleCommandAttribute), false);
                        if (attrs.Length > 0)
                        {
                            var attr = (ConsoleCommandAttribute)attrs[0];
                            if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string[]))
                            {
                                var command = new ReflectionConsoleCommand(attr.Command, attr.Description, method);
                                RegisterCommand(command);
                            }
                            else
                            {
                                LogEx.LogWarning($"Method '{method.Name}' in '{type.Name}' has ConsoleCommandAttribute but does not match the required signature. It should have a single parameter of type string[].");
                            }
                        }
                    }
                }
            }
        }
        
    }

    /// <summary>
    /// 디버그 콘솔의 메인 클래스입니다.
    /// 콘솔의 Model 역할을 합니다.
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
            PrintInternal(type, condition);
        }
        
        private void PrintInternal(LogType type, string message)
        {
            _window?.Print(type, message);
        }
        
        public static void Print(string message)
        {
            Instance.PrintInternal(LogType.Log, message);
        }
        public static void Print(LogType type, string message)
        {
            Instance.PrintInternal(type, message);
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
        
        public static void SetLogLevel(LogLevel level)
        {
            Instance.LogPrintLevel = level;
        }
        
    }
}