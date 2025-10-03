using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.DebugConsole
{
    public interface IConsoleCommand
    {
        public string Command { get; }
        public string Description { get; }
        public void Execute(string[] args);
        
        public void AutoComplete(string[] args, ref List<string> suggestions) { }
    }

    public interface IConsoleWindow
    {
        public void Open();
        public void Close();
        public void Toggle();
        public bool IsOpen { get; }
    }

    public class Console
    {
        private static Console _console;
        public static Console Instance => _console ??= new Console();
        
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeCommands()
        {
            _console = new Console();
        }


        public void GetAutoCompleteSuggestions(string[] input, ref List<string> suggestions)
        {
            suggestions.Clear();
            
        }
    }
}