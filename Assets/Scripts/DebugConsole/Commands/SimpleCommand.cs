using Cardevil.Utils;
using System;

namespace Cardevil.DebugConsole.Commands
{


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
    
    public class SimpleCommand<T> : IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        private readonly Action<T> _action;

        public SimpleCommand(string command, string description, Action<T> action)
        {
            Command = command;
            Description = description;
            _action = action;
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