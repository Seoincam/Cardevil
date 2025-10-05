using System.Collections.Generic;

namespace Cardevil.DebugConsole.Commands
{
    public interface IConsoleCommand
    {
        string Command { get; }
        string Description { get; }
        
        
        string Signature => Command + " " + (Description ?? "");
        void Execute(string[] args);
        
        void AutoComplete(string[] args, ref List<string> suggestions) { }
    }
}