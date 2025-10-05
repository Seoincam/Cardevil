using Cardevil.Attributes;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Cardevil.DebugConsole
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public class ConsoleUI : MonoBehaviour, IConsoleWindow
    {
        
        /*
         * 기본 설정
         */
        [SerializeField] private GameObject consolePanel;
        [SerializeField, VisibleOnly] private bool _isInitialized = false;
        [SerializeField] private bool _isOpen = false;
        [SerializeField,Tooltip("자동 완성 기능 사용 여부")] private bool _useAutoComplete = false;
        
        
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
        /// <summary>
        /// 해당 텍스트 변경이 자동완성 요청에 의한 것인지 여부.
        /// </summary>
        private bool _wasAutoCompleteJustRequested = false;
        private int _autoCompleteIndex = 0;
        private List<string> _suggestions = new List<string>();


        
        private void Reset()
        {
            consolePanel = this.gameObject;
            trueRoot = GetComponent<UIDocument>().rootVisualElement;
        }

        private void Awake()
        {
            #if DO_NOT_USE_DEBUG_CONSOLE
            Destroy(this.gameObject);
            #else
            if (consolePanel == null)
            {
                consolePanel = this.gameObject;
            }
            if (trueRoot == null)
            {
                var uiDocument = GetComponent<UIDocument>();
                if (uiDocument != null)
                {
                    uiDocument.enabled = true;
                    trueRoot = uiDocument.rootVisualElement;
                }
                else
                {
                    LogEx.LogError("ConsoleUI: No UIDocument found!");
                    return;
                }
                
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
            #endif
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

            UniTask.DelayFrame(1).ContinueWith(() =>
            {
                if (!IsOpen) return;
                textField.Focus();
            });
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

        public void Message(string message)
        {
            Message(MessageType.Default, message);
        }
        public void Message(MessageType type, string message)
        {
            var label = new Label(message);
            label.AddToClassList("log-line");
            label.AddToClassList("message");
            label.AddToClassList(type.UssTag);
            historyContainer.Add(label);
            historyContainer.ScrollTo(label);
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
        
        
        /// <summary>
        /// 자동완성 요청시 호출됩니다.
        /// 이미 자동완성 제안이 있다면 다음 제안을 선택합니다.
        /// </summary>
        /// <param name="input"></param>
        public void RequestAutoComplete(string input)
        {
            if (_isAutoCompleteRequested)
            {
                // 기존 제안이 있는 경우
                if (_suggestions.Count == 0) return;
                _autoCompleteIndex = (_autoCompleteIndex + 1) % _suggestions.Count;
                _wasAutoCompleteJustRequested = true;
                var previewLabels = previewContainer.Query<Label>().ToList();
                for (int i = 0; i < previewLabels.Count; i++)
                {
                    if (i == _autoCompleteIndex)
                    {
                        previewLabels[i].AddToClassList("selected");
                    }
                    else
                    {
                        previewLabels[i].RemoveFromClassList("selected");
                    }
                }
                SetInputField(_suggestions[_autoCompleteIndex]);
                textField.Focus();
            }
            else
            {
                // 새로운 자동완성 요청
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
            foreach (var suggestion in _suggestions)
            {
                var label = new Label(suggestion);
                label.AddToClassList("suggestion");
                previewContainer.Add(label);
            }
        }
        
        public void SetInputField(string input)
        {
            textField.value = input;
        }

        public void ExecuteCommand(string input)
        {
            Console.ExecuteCommand(input);
        }
        
        private void OnSubmit(string input)
        {
            _cachedAutoCompleteInput = "";
            textField.value = "";
            _isAutoCompleteRequested = false;
            _autoCompleteIndex = 0;
            _suggestions.Clear();
            previewContainer.Clear();
            textField.Focus();
            Message(MessageType.Gray,$"> {input}");
            ExecuteCommand(input);
        }
        
        /// <inheritdoc cref="OnTextChanged(string)"/>
        private void OnTextChanged(ChangeEvent<string> input)
        {
            OnTextChanged(input.newValue);
        }
        /// <summary>
        /// 텍스트 변경시 호출됩니다.
        /// 자동완성 제안을 갱신합니다.
        /// </summary>
        /// <param name="input">새로 바뀐 텍스트</param>
        private void OnTextChanged(string input)
        {
            if(!_useAutoComplete)
                return;
            if (_wasAutoCompleteJustRequested)
            {
                _wasAutoCompleteJustRequested = false;
                // 자동완성 요청에 의한 텍스트 변경이므로 무시
                return;
            }
            _cachedAutoCompleteInput = input;
            if(string.IsNullOrWhiteSpace(input))
            {
                _isAutoCompleteRequested = false;
                _autoCompleteIndex = 0;
                _suggestions.Clear();
                previewContainer.Clear();
                return;
            }
            RefreshAutoCompleteSuggestions();
        }
        
        /// <inheritdoc cref="OnTextFieldUnfocused()"/>
        private void OnTextFieldUnfocused(FocusOutEvent evt)
        {
            // LogEx.Log("TextField Unfocused");
            OnTextFieldUnfocused();
        }
        
        /// <summary>
        /// 텍스트 필드가 포커스를 잃었을 때 호출됩니다.
        /// </summary>
        private void OnTextFieldUnfocused()
        {
            previewContainer.Clear();
            _isAutoCompleteRequested = false;
            _autoCompleteIndex = 0;
        }
        
        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            // LogEx.Log($"KeyDown: {evt.keyCode}");
            if (evt.keyCode == KeyCode.Tab)
            {
                if(!_useAutoComplete)
                    return;
                evt.StopPropagation();
                evt.StopImmediatePropagation();
                FocusController focusController = textField.panel.focusController;
                focusController.IgnoreEvent(evt);
                RequestAutoComplete(CurrentInput);
            }
            else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                LogEx.Log($"Submitted: {CurrentInput}, {textField.value}");
                OnSubmit(CurrentInput);
            }
        }
        
        private void OnToggleKeyPressed(InputAction.CallbackContext context)
        {
            Toggle();
        }
        
        [Preserve, ConsoleCommand("printAllLogTypes", "Print all log types for testing purposes")]
        private static void PrintAllLogTypes()
        {
            Console console = Console.Instance;
            if (console == null)
            {
                LogEx.LogError("Console instance is null.");
                return;
            }
            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                Console.Print(type, $"Log of type {type}");
            }
            
            foreach (var msgType in MessageType.Default.AllTypes)
            {
                Console.Message(msgType, $"Message of type {msgType}");
            }
        }
    }
}