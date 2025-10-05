using Cardevil.Utils;
using System;

namespace Cardevil.DebugConsole.Commands
{


    /// <summary>
    /// 매개변수가 없는 간단한 Command 클래스입니다.
    /// </summary>
    public class SimpleCommand : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly Action _action;

        public SimpleCommand(string command, string description, Action action)
        {
            Command = command;
            Description = description;
            _action = action;
        }

        public void Execute(string[] args)
        {
            _action.Invoke();
        }
    }
    
    /// <summary>
    /// 매개변수가 하나인 간단한 Command 클래스입니다.
    /// 자동으로 타입 변환을 시도합니다.
    /// </summary>
    public class SimpleCommand<T> : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly Action<T> _action;
        private readonly string _signature;
        public string Signature => _signature;

        public SimpleCommand(string command, string description, Action<T> action, string signature = null)
        {
            Command = command;
            Description = description;
            _action = action;
            _signature = signature ?? $"{command} <{typeof(T).Name}>";
        }

        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                LogEx.LogError($"Command '{Command}' requires an argument of type {typeof(T).Name}");
                return;
            }

            try
            {
                T arg = (T)Convert.ChangeType(args[0], typeof(T));
                _action.Invoke(arg);
            }
            catch (Exception e)
            {
                LogEx.LogError($"Failed to execute command '{Command}': {e.Message}");
            }
        }
    }
}