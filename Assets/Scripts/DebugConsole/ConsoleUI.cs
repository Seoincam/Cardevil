using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Cardevil.DebugConsole
{
    public class ConsoleUI : MonoBehaviour, IConsoleWindow
    {
        [SerializeField] private GameObject consolePanel;
        public bool IsOpen => consolePanel.activeSelf;
        public Console Console => Console.Instance;
        
        [Space]
        private VisualElement root = null;
        TextField textField = null;
        VisualElement previewContainer = null;
        private VisualElement historyContainer = null;
        
        private string _currentInput = "";
        private bool _isAutoCompleteRequested = false;
        private int _autoCompleteIndex = 0;
        private List<string> _suggestions = new List<string>();

        private void Reset()
        {
            consolePanel = this.gameObject;
            root = GetComponent<UIDocument>().rootVisualElement;
        }

        private void Awake()
        {
            if (consolePanel == null)
            {
                consolePanel = this.gameObject;
            }
            if (root == null)
            {
                root = GetComponent<UIDocument>().rootVisualElement;
            }
            
            textField = root.Q<TextField>("InputField");
            previewContainer = root.Q<VisualElement>("PreviewContainer");
            historyContainer = root.Q<VisualElement>("HistoryContainer");

            textField.isDelayed = true;
            Close();
        }

        private void OnEnable()
        {
            RegisterHandlers();
            textField.Focus();
        }
        private void OnDisable()
        {
            UnregisterHandlers();
        }


        private void RegisterHandlers()
        {
            textField.RegisterValueChangedCallback(evt => OnTextChanged(evt.newValue));
            textField.RegisterCallback<FocusOutEvent>(evt => _isAutoCompleteRequested = false);
            textField.RegisterCallback<KeyDownEvent>(evt =>  OnTextFieldKeyDown(evt));
        }
        
        private void UnregisterHandlers()
        {
            textField.UnregisterValueChangedCallback(evt => OnTextChanged(evt.newValue));
            textField.UnregisterCallback<FocusOutEvent>(evt => _isAutoCompleteRequested = false);
            textField.UnregisterCallback<KeyDownEvent>(evt =>  OnTextFieldKeyDown(evt));
        }
        
        

        public void Open()
        {
            consolePanel.SetActive(true);
        }

        public void Close()
        {
            consolePanel.SetActive(false);
        }

        public void Toggle()
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
        }

        public void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard.backquoteKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }
        
        public void OnSubmit(string input)
        {
            _currentInput = input;
            ExecuteCommand(input);
            textField.value = "";
            _isAutoCompleteRequested = false;
            _autoCompleteIndex = 0;
            _suggestions.Clear();
            previewContainer.Clear();
        }


        /// <inheritdoc cref="OnTextChanged(string)"/>
        public void OnTextChanged(ChangeEvent<string> input)
        {
            OnTextChanged(input.newValue);
        }
        /// <summary>
        /// 텍스트 변경시 호출됩니다.
        /// 자동완성 제안을 갱신합니다.
        /// </summary>
        /// <param name="input">새로 바뀐 텍스트</param>
        public void OnTextChanged(string input)
        {
            var args = input.Split(' ');
            
            Console.GetAutoCompleteSuggestions(args, ref _suggestions);
    
        }
        
        /// <inheritdoc cref="OnTextFieldUnfocused()"/>
        public void OnTextFieldUnfocused(FocusOutEvent evt)
        {
            OnTextFieldUnfocused();
        }
        
        /// <summary>
        /// 텍스트 필드가 포커스를 잃었을 때 호출됩니다.
        /// </summary>
        public void OnTextFieldUnfocused()
        {
            previewContainer.Clear();
        }
        
        public void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Tab)
            {
                RequestAutoComplete(_currentInput);
            }
            else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnSubmit(_currentInput);
            }
        }
        
        /// <summary>
        /// 자동완성 요청시 호출됩니다.
        /// 이미 자동완성 제안이 있다면 다음 제안을 선택합니다.
        /// </summary>
        /// <param name="input"></param>
        public void RequestAutoComplete(string input)
        {
            if (_isAutoCompleteRequested)
            {
                if (_suggestions.Count == 0) return;
                _autoCompleteIndex = (_autoCompleteIndex + 1) % _suggestions.Count;
                
            }
            else
            {
                _isAutoCompleteRequested = true;
                _autoCompleteIndex = 0;
                _suggestions = new List<string>();
            }
        }
        
        public void SetInputField(string input)
        {
            
        }

        public void ExecuteCommand(string input)
        {
            string[] args = input.Split(' ');
            if (args.Length == 0) return;
            string command = args[0];
            string[] commandArgs = args.Length > 1 ? args[1..] : new string[0];
            
           
        }
    }
}