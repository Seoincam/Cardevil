using Cardevil.Attributes;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace Cardevil.DebugConsole
{
    public class ConsoleUI : MonoBehaviour, IConsoleWindow
    {
        /*
         * 기본 설정
         */
        [SerializeField] private GameObject consolePanel;
        [SerializeField, VisibleOnly] private bool _isInitialized = false;
        [SerializeField] private bool _isOpen = false;
        
        
        [SerializeField] InputAction _toggleConsoleAction;
        
        public bool IsInitialized => _isInitialized;
        public bool IsOpen => _isOpen;
        public Console Console => Console.Instance;
        
        /*
         * UI 관련
         */
        private VisualElement trueRoot = null;
        private VisualElement root = null;
        TextField textField = null;
        
        VisualElement previewContainer = null;
        ScrollView historyContainer = null;
        
        /*
         * 상태
         */
        
        public string CurrentInput => textField.value.TrimEnd();
        
        /*
         * 자동완성 관련
         */
        private string _cachedAutoCompleteInput = "";
        private bool _isAutoCompleteRequested = false;
        private int _autoCompleteIndex = 0;
        private List<string> _suggestions = new List<string>();

        private void Reset()
        {
            consolePanel = this.gameObject;
            trueRoot = GetComponent<UIDocument>().rootVisualElement;
        }

        private void Awake()
        {
            if (consolePanel == null)
            {
                consolePanel = this.gameObject;
            }
            if (trueRoot == null)
            {
                trueRoot = GetComponent<UIDocument>().rootVisualElement;
            }
            
            Console.RegisterWindow(this);
            
            
            root = trueRoot.Q<VisualElement>("Root");
            LogEx.Log($"ConsoleUI: {trueRoot}, {root}");
            
            
            textField = trueRoot.Q<TextField>("InputField");
            previewContainer = trueRoot.Q<VisualElement>("PreviewContainer");
            historyContainer = trueRoot.Q<ScrollView>("HistoryContainer");
            historyContainer.AddToClassList("history");
            

            if (_toggleConsoleAction != null)
            {
                _toggleConsoleAction.performed += OnToggleKeyPressed;
                _toggleConsoleAction.Enable();
            }
            else
            {
                Keyboard.current.onTextInput += c =>
                {
                    if (c == '`')
                    {
                        Toggle();
                    }
                };
            }
            
            _isInitialized = true;
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


        private void OnDestroy()
        {
            if (_toggleConsoleAction != null)
            {
                _toggleConsoleAction.performed -= OnToggleKeyPressed;
                _toggleConsoleAction.Disable();
            }

            Console.UnregisterWindow(this);
        }

        private void RegisterHandlers()
        {
            LogEx.Log("RegisterHandlers");
            
            textField.RegisterValueChangedCallback(OnTextChanged);
            textField.RegisterCallback<FocusOutEvent>(OnTextFieldUnfocused);
            textField.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown, TrickleDown.TrickleDown);
        }
        
        private void UnregisterHandlers()
        {
            LogEx.Log("UnregisterHandlers");
            textField.UnregisterValueChangedCallback(OnTextChanged);
            textField.UnregisterCallback<FocusOutEvent>(OnTextFieldUnfocused);
            textField.UnregisterCallback<KeyDownEvent>(OnTextFieldKeyDown, TrickleDown.TrickleDown);
        }
        
        

        public void Open()
        {
            _isOpen = true;
            trueRoot.style.display = DisplayStyle.Flex;
            textField.value = "";
            textField.Focus();
        }

        public void Close()
        {
            _isOpen = false;
            trueRoot.style.display = DisplayStyle.None;
            textField.Blur();
        }
        public void Toggle()
        {
            if (!IsInitialized) return;
            if (IsOpen) Close();
            else Open();
        }
        public void Print(string message)
        {
            Print(LogType.Log, message);
        }
        
        public void Print(LogType type, string message)
        {
            if (!IsInitialized)
                return;
            var label = new Label(message);
            // label.styleSheets 
            label.AddToClassList("log-line");
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    label.AddToClassList("error");
                    break;
                case LogType.Warning:
                    label.AddToClassList("warn");
                    break;
                case LogType.Log:
                default:
                    label.AddToClassList("info");
                    break;
            }
            
            historyContainer.Add(label);
            historyContainer.ScrollTo(label);
        }

        public void ClearHistory()
        {
            if (!IsInitialized)
                return;
            historyContainer.Clear();
        }


        
        public void OnSubmit(string input)
        {
            _cachedAutoCompleteInput = input;
            ExecuteCommand(input);
            textField.value = "";
            _isAutoCompleteRequested = false;
            _autoCompleteIndex = 0;
            _suggestions.Clear();
            previewContainer.Clear();
            textField.Focus();
            Print($"> {input}");
        }
        
        /// <inheritdoc cref="OnTextChanged(string)"/>
        public void OnTextChanged(ChangeEvent<string> input)
        {
            LogEx.Log($"TextChanged: {input.newValue}");
            OnTextChanged(input.newValue);
        }
        /// <summary>
        /// 텍스트 변경시 호출됩니다.
        /// 자동완성 제안을 갱신합니다.
        /// </summary>
        /// <param name="input">새로 바뀐 텍스트</param>
        public void OnTextChanged(string input)
        {
            _cachedAutoCompleteInput = input;
            RefreshAutoCompleteSuggestions();
        }
        
        /// <inheritdoc cref="OnTextFieldUnfocused()"/>
        public void OnTextFieldUnfocused(FocusOutEvent evt)
        {
            LogEx.Log("TextField Unfocused");
            OnTextFieldUnfocused();
        }
        
        /// <summary>
        /// 텍스트 필드가 포커스를 잃었을 때 호출됩니다.
        /// </summary>
        public void OnTextFieldUnfocused()
        {
            previewContainer.Clear();
            _isAutoCompleteRequested = false;
            _autoCompleteIndex = 0;
        }
        
        public void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            LogEx.Log($"KeyDown: {evt.keyCode}");
            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopPropagation();
                evt.StopImmediatePropagation();
                FocusController focusController = textField.panel.focusController;
                focusController.IgnoreEvent(evt);
                RequestAutoComplete(CurrentInput);
            }
            else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                OnSubmit(CurrentInput);
                LogEx.Log($"Submitted: {CurrentInput}, {textField.value}");
            }
        }
        
        public void OnToggleKeyPressed(InputAction.CallbackContext context)
        {
            Toggle();
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
                _suggestions.Clear();
                var args = input.Split(' ');
                Console.GetAutoCompleteSuggestions(args, ref _suggestions);
            }
        }

        /// <summary>
        /// 자동완성 제안을 갱신합니다.
        /// </summary>
        public void RefreshAutoCompleteSuggestions()
        {
            previewContainer.Clear();
            if (_suggestions.Count == 0) return;
            Console.GetAutoCompleteSuggestions(_cachedAutoCompleteInput.Split(' '), ref _suggestions);

        }
        
        public void SetInputField(string input)
        {
            textField.value = input;
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