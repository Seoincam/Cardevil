
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


        private static SimpleCommand ping; 
        private static SimpleCommand<int> setLogLevelCommand;

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
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            /*
             * 수동 등록
             */
            RegisterCommand(setLogLevelCommand);
            RegisterCommand(ping);
            
            
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
    #endif
        
    }
}