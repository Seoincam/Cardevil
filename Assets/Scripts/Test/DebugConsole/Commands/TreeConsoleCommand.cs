using System;
using System.Collections.Generic;

namespace Cardevil.Test.DebugConsole.Commands
{
    /// <summary>
    /// 하위 명령어를 가질 수 있는 트리 구조의 콘솔 명령어입니다.
    /// </summary>
    [Obsolete("아직 테스트되지 않았습니다.")]
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
                return $"{Command} <sub-command>";
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
}