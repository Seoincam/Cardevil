
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cardevil.DebugConsole.Commands
{
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

        private class HelpCommand : IConsoleCommand
        {
            public string Command => "help";
            public string Description => "등록된 모든 명령어를 출력합니다.";
            public string Signature => "help [command]";

            public void Execute(string[] args)
            {
                if(args.Length == 0)
                {
                    var commands = CommandLibrary.GetAllCommands();
                    Console.MessageInfo("Available Commands:");
                    foreach (var command in commands)
                    {
                        Console.MessageDefault($"- {command.Signature}: {command.Description}");
                    }
                    return;
                }
                else
                {
                    string commandName = args[0];
                    if (CommandLibrary.TryGetCommand(commandName, out var command))
                    {
                        Console.MessageInfo( $"Command: {command.Signature}");
                        Console.MessageDefault($"Description: {command.Description}");
                    }
                    else
                    {
                        Console.MessageError($"No help available for unknown command: '{commandName}'");
                    }

                    return;
                }
            }
            
            public void AutoComplete(Span<string> args, ref List<string> suggestions)
            {
                if (args.Length == 0)
                {
                    // 모든 명령어 제안
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        suggestions.Add(cmd.Command);
                    }
                    return;
                }
                
                int argIndex = args.Length - 1;
                var currentArg = args[argIndex];
                if (argIndex == 0)
                {
                    // 첫 번째 인자라면, 등록된 명령어들 중에서
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        if (cmd.Command.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                            suggestions.Add(cmd.Command);
                    }
                }
                // 두 번째 인자부터는 자동완성 없음
            }
        }

        private static SimpleCommand ping; 
        private static SimpleCommand<int> setLogLevelCommand;
        private static HelpCommand helpCommand = new HelpCommand();
        private static RawCommand help2Command;

        static CommandLibrary()
        {
#if !DO_NOT_USE_DEBUG_CONSOLE
            /*
             * 기본 명령어 등록
             * remark : 이곳에 등록된 명령어들은 수동으로 등록됩니다. Initialize() 메서드에 추가하세요.
             */
            ping = new SimpleCommand(
                "ping",
                "콘솔이 정상 작동하는지 확인합니다.",
                () => Console.Print("Pong!")
            );
            setLogLevelCommand = new SimpleCommand<int>(
                "setLogLevel",
                "어떤 유니티 로그를 콘솔에 출력할 지 결정합니다. Usage: setLogLevel <0-4> (0: None, 1: Exception, 2: Error, 3: Warning, 4: Info)",
                (level) =>
                {
                    if (level < 0)
                    {
                        Console.Message(MessageType.Error,"Invalid log level. Minimum is 0 (None).");
                        return;
                    }
                    if(level > (int)LogLevel.Info)
                    {
                        level = (int)LogLevel.Info;
                    }
                    Console.SetLogLevel((LogLevel)level);
                    Console.Print($"Log level set to {(LogLevel)level}");
            });
            
            help2Command = new RawCommand("help2", "등록된 모든 명령어를 출력합니다. (RawCommand 버전)", "help2 [command]",
                action:(args) =>
            {
                if(args.Length == 0)
                {
                    var commands = CommandLibrary.GetAllCommands();
                    Console.MessageInfo("Available Commands:");
                    foreach (var command in commands)
                    {
                        Console.MessageDefault($"- {command.Signature}: {command.Description}");
                    }
                    return;
                }
                else
                {
                    string commandName = args[0];
                    if (CommandLibrary.TryGetCommand(commandName, out var command))
                    {
                        Console.MessageInfo( $"Command: {command.Signature}");
                        Console.MessageDefault($"Description: {command.Description}");
                    }
                    else
                    {
                        Console.MessageError($"No help available for unknown command: '{commandName}'");
                    }

                    return;
                }
            },  autoCompleteAction:(args, suggestions) =>
            {
                if (args.Length == 0)
                {
                    // 모든 명령어 제안
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        suggestions.Add(cmd.Command);
                    }
                    return;
                }
                
                int argIndex = args.Length - 1;
                var currentArg = args[argIndex];
                if (argIndex == 0)
                {
                    // 첫 번째 인자라면, 등록된 명령어들 중에서
                    foreach (var cmd in CommandLibrary.GetAllCommands())
                    {
                        if (cmd.Command.StartsWith(currentArg, StringComparison.OrdinalIgnoreCase))
                            suggestions.Add(cmd.Command);
                    }
                }
                // 두 번째 인자부터는 자동완성 없음
            });
#endif
        }
  

        
        
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
                LogEx.Log($"Registered command: {command.Command} ({command.GetType().Name})");
            }
            _commands[command.Command] = command;
        }
        
        /// <summary>
        /// 콘솔 명령어 등록을 해제합니다.
        /// </summary>
        /// <param name="commandName"></param>
        public static void UnregisterCommand(string commandName)
        {
            if (_commands.Remove(commandName))
            {
                LogEx.Log($"Unregistered command: {commandName}");
            }
            else
            {
                LogEx.LogWarning($"Command '{commandName}' is not registered.");
            }
        }

        /// <summary>
        /// 콘솔 명령어를 이름으로 검색합니다.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool TryGetCommand(string commandName, out IConsoleCommand command)
        {
#if !DO_NOT_USE_DEBUG_CONSOLE
            return _commands.TryGetValue(commandName, out command);
#else
            command = null;
            return false;
#endif
        }

        /// <summary>
        /// 등록된 모든 콘솔 명령어를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IConsoleCommand> GetAllCommands()
        {
            return _commands.Values;
        }
        

    #if !DO_NOT_USE_DEBUG_CONSOLE
        /// <summary>
        /// 애플리케이션 시작 시점에 호출되어, 모든 콘솔 명령어를 자동으로 등록합니다.
        /// 1. 수동으로 등록할 명령어가 있다면 이 메서드에 추가합니다.
        /// 2. ConsoleCommandAttribute가 붙은 모든 static 메서드를 찾아 등록합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            /*
             * 수동 등록
             */
            RegisterCommand(setLogLevelCommand);
            RegisterCommand(ping);
            RegisterCommand(helpCommand);
            RegisterCommand(help2Command);
            
            
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
                            if(method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string[]))
                            {
                                // RawReflectionCommand 생성
                                var rawCommand = new RawReflectionCommand(attr.Command, attr.Description, method, attr.Signature);
                                if(attr.Arg0AutoComplete != null)
                                    rawCommand.SetArg0AutoComplete(attr.Arg0AutoComplete);
                                RegisterCommand(rawCommand);
                            }else{
                                // ReflectionCommand 생성
                                bool success = ReflectionCommand.Create(attr.Command, attr.Description, method, attr.Signature, out ReflectionCommand consoleCommand);
                                if (success)
                                {
                                    if(attr.Arg0AutoComplete != null)
                                        consoleCommand.SetArg0AutoComplete(attr.Arg0AutoComplete);
                                    RegisterCommand(consoleCommand);
                                }
                                else
                                {
                                    LogEx.LogWarning($"Failed to create console command for method \"{type.FullName}.{method.Name}\". \n Make sure the method is static and all parameter types are supported.");
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    #endif
        
    }
}