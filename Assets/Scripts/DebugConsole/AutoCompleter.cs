using Cardevil.DebugConsole.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cardevil.DebugConsole
{
    public class AutoCompleter 
    {
        private string _chachedInput = "I_AM_GROOT";
        private List<string> _cachedSuggestions = new List<string>();
        private int _currentIndex = -1;
        
        private int _completTarget = -1; // 0: command part, 1...: arg part
        
        public string ChachedInput => _chachedInput;
        public string CurrentSuggestion => GetCurrentFullSuggestion();
        public IReadOnlyList<string> CurrentSuggestions => _cachedSuggestions.AsReadOnly();
        public int CurrentIndex => _currentIndex;
        public int CurrentTarget => _completTarget;
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
                GetSuggestions(_chachedInput, ref _cachedSuggestions);
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
        public string GetCurrentFullSuggestion()
        {
            if (_currentIndex < 0 || _currentIndex >= _cachedSuggestions.Count)
                return null;
            return GetFullSuggestion(_currentIndex);
        }
        public string GetNextFullSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex + 1) % _cachedSuggestions.Count;
            return GetFullSuggestion(_currentIndex);
        }
        public string GetPreviousFullSuggestion()
        {
            if (_cachedSuggestions.Count == 0)
                return null;
            _currentIndex = (_currentIndex - 1 + _cachedSuggestions.Count) % _cachedSuggestions.Count;
            return GetFullSuggestion(_currentIndex);
        }
        
        private string GetFullSuggestion(int index)
        {
            if (index < 0 || index >= _cachedSuggestions.Count)
                return null;
            if (_completTarget == 0)
            {
                return _cachedSuggestions[index];
            }else if (_completTarget > 0)
            {
                string[] parts = _chachedInput.Split(' ');
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i <= _completTarget; i++)
                {
                    sb.Append(parts[i]);
                    sb.Append(' ');
                }
                sb.Append(_cachedSuggestions[index]);
                return sb.ToString();
            }
            else
            {
                return _cachedSuggestions[index];
            }
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
        
        public void GetSuggestions(string input, ref List<string> suggestions)
        {
            if(suggestions == null)
                suggestions = new List<string>();
            else
                suggestions.Clear();
            // TODO : 배열 복사가 빈번하게 일어남
            string[] parts = input.Split(' ');
            if (parts.Length == 0)
            {
                return;
            }
            string commandPart = parts[0];
            Span<string> argsPart = new Span<string>(parts, 1, parts.Length - 1);
            for (int i = 1; i < parts.Length; i++)
                argsPart[i - 1] = parts[i];
            
            _completTarget = argsPart.Length;

            // 명령어부분
            if (_completTarget == 0)
            {
                foreach (var cmd in CommandLibrary.GetAllCommands())
                {
                    if (cmd.Command.StartsWith(commandPart))
                    {
                        suggestions.Add(cmd.Command);
                    }
                }
                return;
            }
            
            // 인자부분
            IConsoleCommand command;
            if (!CommandLibrary.TryGetCommand(commandPart, out command))
            {
                return;
            }
            command.AutoComplete(argsPart, ref suggestions);

            return;
        }
        
    }
}