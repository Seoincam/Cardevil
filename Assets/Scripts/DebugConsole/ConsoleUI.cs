using Cardevil.Attributes;
using Cardevil.ScreenHelper;
using Cardevil.UIToolkit;
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
        private static ConsoleUI _instance;
        public static ConsoleUI Instance => _instance;
        /*
         * 기본 변수
         */
        [Header("기본 설정")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField, VisibleOnly] private bool _isInitialized = false;
        [SerializeField] private bool _isOpen = false;
        [SerializeField,Tooltip("자동 완성 기능 사용 여부")] private bool _useAutoComplete = false;
        [SerializeField] private bool autoScrollToBottomOnNewMessage = true;
        [SerializeField] private bool autoScrollToBottomOnNewPrint = true;
        [Header("단축키 설정")]
        [SerializeField] InputAction _toggleConsoleAction;
        // [SerializeField] InputAction _autoCompleteConsoleAction;
        [Header("크기 제한")]
        [SerializeField] private Vector2 minSize = new Vector2(360, 200);
        [SerializeField] private Vector2 maxSize = new Vector2(1920, 1200);
        public bool IsInitialized => _isInitialized;
        public bool IsOpen => _isOpen;
        public Console Console => Console.Instance;
        
        /*
         * UI 관련
         */
        private VisualElement trueRoot = null;
        private VisualElement root = null;
        TextField textField = null;
        
        ScrollView previewContainer = null;
        ScrollView historyContainer = null;
        
        /*
         * 상태
         */
        
        private bool _shouldScrollToBottom = false;
        public string CurrentInput => textField.value.TrimEnd();
        
        /*
         * 자동완성 관련
         */
        [Header("자동완성 관련")]
        private AutoCompleter _autoCompleter = new AutoCompleter();
        /// <summary>
        /// 해당 텍스트 변경이 자동완성 요청에 의한 것인지 여부.
        /// </summary>
        private bool _wasAutoCompleteJustRequested = false;
        private List<string> _submittedCommands = new List<string>();
        private int _currentRecallIndex = -1;
        public IReadOnlyList<string> SubmittedCommands => _submittedCommands.AsReadOnly();
        
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
            /*
             * 기초 설정
             */
            if (_instance != null && _instance != this)
            {
                LogEx.LogWarning("Another instance of ConsoleUI already exists. Destroying this one.");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
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
            
            
            /*
             * UI 요소 찾기
             */
            root = trueRoot.Q<VisualElement>("Root");
            LogEx.Log($"ConsoleUI: {trueRoot}, {root}");
            
            
            textField = trueRoot.Q<TextField>("InputField");
            previewContainer = trueRoot.Q<ScrollView>("PreviewContainer");
            historyContainer = trueRoot.Q<ScrollView>("HistoryContainer");
            historyContainer.AddToClassList("history");
            
            textField.value = "";
            
            /*
             * Manipulator 설정
             */
            var resizeEast = trueRoot.Q<VisualElement>("resize-east");
            var resizeSouth = trueRoot.Q<VisualElement>("resize-south");
            var resizeSouthEast = trueRoot.Q<VisualElement>("resize-southeast");

            var eastManipulator = new ResizeManipulator(resizeEast, root, ResizeEdge.East);
            var southManipulator = new ResizeManipulator(resizeSouth, root, ResizeEdge.South);
            var southEastManipulator = new ResizeManipulator(resizeSouthEast, root, ResizeEdge.SouthEast);
            
            void SetResizeMinMaxSize(Vector2 min, Vector2 max)
            {
                eastManipulator.MinSize = southManipulator.MinSize = southEastManipulator.MinSize = min;
                eastManipulator.MaxSize = southManipulator.MaxSize = southEastManipulator.MaxSize = max;
            }
            SetResizeMinMaxSize(minSize, maxSize);
            eastManipulator.ClampToParentBounds = southManipulator.ClampToParentBounds = southEastManipulator.ClampToParentBounds = true;
            
            var resolutionChangeWatcher = ResolutionWatcher.Instance;
            if (resolutionChangeWatcher != null)
            {
                resolutionChangeWatcher.OnResolutionChanged += (newSize) =>
                {
                    SetResizeMinMaxSize(
                        new Vector2(Mathf.Min(minSize.x, newSize.x), Mathf.Min(minSize.y, newSize.y)),
                        new Vector2(Mathf.Min(maxSize.x, newSize.x), Mathf.Min(maxSize.y, newSize.y))
                    );
                };
            }
            
            resizeEast.AddManipulator(eastManipulator);
            resizeSouth.AddManipulator(southManipulator);
            resizeSouthEast.AddManipulator(southEastManipulator);
            
            var topBar = trueRoot.Q<VisualElement>("TopBar");
            var dragManipulator = new DragManipulator(topBar, root);
            dragManipulator.ClampToParentBounds = true;
            dragManipulator.Padding = new RectOffset(5,5,5,5);
            topBar.AddManipulator(dragManipulator);
            
            /*
             * 토글 단축키 설정
             */
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

        private void LateUpdate()
        {
            if (_shouldScrollToBottom)
            {
                _shouldScrollToBottom = false;
                ScrollToBottom();
            }
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
            
            ScrollToBottom();

            textField.schedule.Execute(() =>
            {
                if (!IsOpen) return;
                textField.Focus();
                OnTextChanged(CurrentInput);
                // UpdatePreviewContainer();
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
            if (autoScrollToBottomOnNewMessage)
            {
                _shouldScrollToBottom = true;
            }
        }
        
        private void Message(MessageType type, string message, params string[] additionalClasses)
        {
            var label = new Label(message);
            label.AddToClassList("log-line");
            label.AddToClassList("message");
            label.AddToClassList(type.UssTag);
            foreach (var cls in additionalClasses)
            {
                label.AddToClassList(cls);
            }
            
            historyContainer.Add(label);
            historyContainer.ScrollTo(label);
            if (autoScrollToBottomOnNewMessage)
            {
                _shouldScrollToBottom = true;
            }
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
            if (autoScrollToBottomOnNewPrint)
            {
                _shouldScrollToBottom = true;
            }
        }

        /// <summary>
        /// 히스토리를 모두 지웁니다.
        /// 실제로 Label 오브젝트를 View에서 제거합니다.
        /// </summary>
        public void ClearHistory()
        {
            if (!IsInitialized)
                return;
            historyContainer.Clear();
        }
        
        /// <summary>
        /// 스크롤을 맨 아래로 이동합니다.
        /// </summary>
        public void ScrollToBottom()
        {
            ScrollView scrollView = Instance.historyContainer;
            Scroller scroller = scrollView.verticalScroller;
            scroller.value = scroller.highValue > 0 ? scroller.highValue : 0;
        }
        
        public void SetOpacity(float opacity)
        {
            root.style.opacity = Mathf.Clamp01(opacity);
        }
        
        /// <summary>
        /// 자동완성 요청시 호출됩니다.
        /// 이미 자동완성 제안이 있다면 다음 제안을 선택합니다.
        /// </summary>
        /// <param name="input"></param>
        public void RequestAutoComplete(string input)
        {
            if(!_useAutoComplete)
                return;
            if (_autoCompleter.SuggestionCount == 0)
            {
                _autoCompleter.TextInputChanged(input);
                if (_autoCompleter.SuggestionCount == 0)
                {
                    // 제안 없음
                    previewContainer.Clear();
                    return;
                }
            }
            string suggestion = _autoCompleter.GetNextFullSuggestion();
            if (suggestion != null)
            {
                _wasAutoCompleteJustRequested = true;
                SetInputField(suggestion);
                UpdatePreviewContainer();
            }
            else
            {
                previewContainer.Clear();
            }
            textField.cursorIndex = textField.value.Length;
            textField.selectIndex = textField.value.Length;
        }

        
        public void SetInputField(string input)
        {
            textField.value = input;
        }

        public void ExecuteCommand(string input)
        {
            Console.ExecuteCommand(input);
        }

        private void UpdatePreviewContainer()
        {
            previewContainer.Clear();
            if (_autoCompleter.SuggestionCount == 0)
                return;

            // previewContainer가 ScrollView일 수도 있으므로 캐스팅

            var suggestions = _autoCompleter.GetAllSuggestions();
            Label selectedLabel = null;

            foreach (var suggestion in suggestions)
            {
                var label = new Label(suggestion);
                label.AddToClassList("suggest"); 
                if (suggestion == _autoCompleter.GetCurrentSuggestion())
                {
                    label.AddToClassList("selected");
                    selectedLabel = label;
                }
                // 클릭 시 바로 선택/실행하도록 이벤트 등록
                label.RegisterCallback<ClickEvent>(_ =>
                {
                    LogEx.Log($"Clicked suggestion: {suggestion}");
                    _autoCompleter.SetCurrentSuggestion(suggestion);
                    _wasAutoCompleteJustRequested = true;
                    SetInputField(suggestion);
                    label.schedule.Execute(() =>
                    {
                        textField.Focus();
                        textField.cursorIndex = textField.value.Length;
                        textField.selectIndex = textField.value.Length;
                        UpdatePreviewContainer();
                    }).ExecuteLater(1);
                });
                previewContainer.Add(label);
            }
            
            if (previewContainer != null && selectedLabel != null)
            {
                previewContainer.schedule.Execute(() =>
                {
                    previewContainer.ScrollTo(selectedLabel);
                });
            }
        }

        
        private void Recall(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= _submittedCommands.Count)
                return;
            _currentRecallIndex = targetIndex;
            SetInputField(_submittedCommands[_currentRecallIndex]);
            textField.cursorIndex = textField.value.Length;
            textField.selectIndex = textField.value.Length;
        }
        
        public void RecallPrevious()
        {
            if (_submittedCommands.Count == 0)
                return;
            if (_currentRecallIndex == -1)
            {
                _currentRecallIndex = _submittedCommands.Count - 1;
            }
            else
            {
                _currentRecallIndex--;
                if (_currentRecallIndex < 0)
                    _currentRecallIndex = 0;
            }
            Recall(_currentRecallIndex);
        }
        
        public void RecallNext()
        {
            if (_submittedCommands.Count == 0)
                return;
            if (_currentRecallIndex == -1)
            {
                _currentRecallIndex = 0;
            }
            else
            {
                _currentRecallIndex++;
                if (_currentRecallIndex >= _submittedCommands.Count)
                    _currentRecallIndex = _submittedCommands.Count - 1;
            }
            Recall(_currentRecallIndex);
        }
        
        /// <summary>
        /// 입력이 제출되었을 때 호출됩니다.
        /// </summary>
        /// <param name="input"></param>
        private void OnSubmit(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;
            textField.value = "";
            _autoCompleter.Clear();
            _wasAutoCompleteJustRequested = false;
            previewContainer.Clear();
            
            Message(MessageType.Gray,$"> {input}");
            ExecuteCommand(input);
            _submittedCommands.Add(input);
            // 기다렸다가 스크롤을 맨 아래로 이동
            historyContainer.schedule.Execute(() =>
            {
                ScrollToBottom();
                textField.Focus();
            }).ExecuteLater(5);
            _currentRecallIndex = -1;
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
            // LogEx.Log($"TextChanged: {input}");
            if(!_useAutoComplete)
                return;
            if (_wasAutoCompleteJustRequested)
            {
                _wasAutoCompleteJustRequested = false;
                // 자동완성 요청에 의한 텍스트 변경이므로 무시
                return;
            }
            _autoCompleter.TextInputChanged(input);
            // if(string.IsNullOrWhiteSpace(input))
            // {
            //     previewContainer.Clear();
            //     return;
            // }
            UpdatePreviewContainer();
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
            // previewContainer.Clear();
            // _autoCompleter.Clear();
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
                // LogEx.Log($"Submitted: {CurrentInput}, {textField.value}");
                OnSubmit(CurrentInput);
            }
            else if (evt.keyCode == KeyCode.UpArrow)
            {
                evt.StopPropagation();
                evt.StopImmediatePropagation();
                FocusController focusController = textField.panel.focusController;
                focusController.IgnoreEvent(evt);
                RecallPrevious();
            }
            else if (evt.keyCode == KeyCode.DownArrow)
            {
                evt.StopPropagation();
                evt.StopImmediatePropagation();
                FocusController focusController = textField.panel.focusController;
                focusController.IgnoreEvent(evt);
                
                RecallNext();
            }
        }
        
        private void OnToggleKeyPressed(InputAction.CallbackContext context)
        {
            Toggle();
        }
        
        [Preserve, ConsoleCommand("printAllLogTypes", "Print all log types for testing purposes")]
        private static void PrintAllLogTypesCommand()
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
        
        [Preserve, ConsoleCommand("clear", "Clears the console window.")]
        private static void ClearCommand()
        {
            Instance.ClearHistory();
        }

        [Preserve, ConsoleCommand("autoScroll", "새로운 출력이 있을 때 자동으로 스크롤을 맨 아래로 내립니다.")]
        private static void ToggleAutoScrollCommand()
        {
            if (Instance == null)
                return;
            Instance.autoScrollToBottomOnNewMessage = !Instance.autoScrollToBottomOnNewMessage;
            Instance.autoScrollToBottomOnNewPrint = Instance.autoScrollToBottomOnNewMessage;
            Console.MessageInfo($"Auto scroll to bottom on new message is now {(Instance.autoScrollToBottomOnNewMessage ? "enabled" : "disabled")}");
        }

        [Preserve, ConsoleCommand("setOpacity", "콘솔의 투명도를 설정합니다", "setOpacity <0.0 - 1.0>")]
        private static void SetOpacityCommand(float opacity)
        {
            var clamped = Mathf.Clamp01(opacity);
            Instance.SetOpacity(clamped);
            Console.MessageInfo($"Console opacity set to {clamped}");
        }
    }
}