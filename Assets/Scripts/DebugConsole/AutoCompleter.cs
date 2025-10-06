using Cardevil.DebugConsole.Commands;
using System.Collections.Generic;

namespace Cardevil.DebugConsole
{
    public class AutoCompleter 
    {
        private string _chachedInput = "";
        private List<string> _cachedSuggestions = new List<string>();
        private int _currentIndex = -1;
        
        public string ChachedInput => _chachedInput;
        public string CurrentSuggestion => GetCurrentSuggestion();
        public IReadOnlyList<string> CurrentSuggestions => _cachedSuggestions.AsReadOnly();
        public int CurrentIndex => _currentIndex;
        public int SuggestionCount => _cachedSuggestions.Count;
        public bool HasSuggestions => _cachedSuggestions.Count > 0;
        
        public AutoCompleter()
        {
            
        }
        
        public void Clear()
        {
            _chachedInput = "";
            _cachedSuggestions.Clear();
            _currentIndex = -1;
        }
        
        public void TextInputChanged(string input)
        {
            if (input != _chachedInput)
            {
                _chachedInput = input;
                _cachedSuggestions = GetSuggestions(input);
                _currentIndex = -1;
            }
        }
        public string GetCurrentSuggestion()
        {
            if (_currentIndex < 0 || _currentIndex >= _cachedSuggestions.Count)
                return null;
            return _cachedSuggestions[_currentIndex];
        }
        public string GetNextSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex + 1) % _cachedSuggestions.Count;
            return _cachedSuggestions[_currentIndex];
        }
        public string GetPreviousSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex - 1 + _cachedSuggestions.Count) % _cachedSuggestions.Count;
            return _cachedSuggestions[_currentIndex];
        }
        
        public bool SetCurrentIndex(int index)
        {
            if (index < 0 || index >= _cachedSuggestions.Count)
                return false;
            _currentIndex = index;
            return true;
        }

        public bool SetCurrentSuggestion(string suggestion)
        {
            int index = _cachedSuggestions.IndexOf(suggestion);
            if (index == -1)
                return false;
            _currentIndex = index;
            return true;
        }
        
        public IReadOnlyList<string> GetAllSuggestions()
        {
            return _cachedSuggestions.AsReadOnly();
        }
        
        public List<string> GetSuggestions(string input)
        {
            // TODO : 배열 복사가 빈번하게 일어남
            List<string> suggestions = new List<string>();
            string[] parts = input.Split(' ');
            if (parts.Length == 0)
                return suggestions;
            string commandPart = parts[0];
            string[] argsPart = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
                argsPart[i - 1] = parts[i];
            
            int completTarget = argsPart.Length;

            // 명령어부분
            if (completTarget == 0)
            {
                foreach (var cmd in CommandLibrary.GetAllCommands())
                {
                    if (cmd.Command.StartsWith(commandPart))
                    {
                        suggestions.Add(cmd.Command);
                    }
                }
                return suggestions;
            }
            
            // 인자부분
            foreach (var cmd in CommandLibrary.GetAllCommands())
            {
                if (cmd.Command.StartsWith(commandPart))
                {
                    cmd.AutoComplete(argsPart, ref suggestions);
                }
            }
            return suggestions;
        }
        
    }
}