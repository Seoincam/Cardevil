using Cardevil.Core.Utils;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class MainToolbar : VisualElement
    {
        public event Action AddRelicClicked;
        public event Action DownloadClicked;
        public event Action SyncClicked;
        
        public event Action<RelicEditorWindow.AlignMode> AlignChanged;
        public event Action<string> KeywordChanged;

        /// <summary>
        /// 시트, 로컬, 미싱.
        /// </summary>
        public event Action<bool, bool, bool> DataSourceChanged;
        
        private readonly Button _addRelicButton;
        private readonly Button _downloadSheetButton;
        private readonly Button _syncSheetButton;
        private readonly EnumField _alignDropDown;
        private readonly TextField _searchField;
        private readonly ToggleButtonGroup _sourceFilter;
        
        public MainToolbar()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/MainToolbar/MainToolbar.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }
            
            visualTree.CloneTree(this);
            
            _addRelicButton = this.Q<Button>("AddRelic");
            _syncSheetButton = this.Q<Button>("SyncSheet");
            _downloadSheetButton = this.Q<Button>("DownloadSheet");
            _alignDropDown = this.Q<EnumField>("AlignDropDown");
            _searchField = this.Q<TextField>("SearchField");
            _sourceFilter = this.Q<ToggleButtonGroup>("SourceFilter");
            
            _addRelicButton.clicked += () => AddRelicClicked?.Invoke();
            _syncSheetButton.clicked += () => SyncClicked?.Invoke();
            _downloadSheetButton.clicked += () => DownloadClicked?.Invoke();
            _alignDropDown.RegisterValueChangedCallback(evt => AlignChanged?.Invoke((RelicEditorWindow.AlignMode)evt.newValue));
            _searchField.RegisterValueChangedCallback(evt => KeywordChanged?.Invoke(evt.newValue.ToString()));
            
            var initialState = _sourceFilter.value;
            initialState[0] = true;
            initialState[1] = true;
            initialState[2] = true;
            _sourceFilter.SetValueWithoutNotify(initialState);

            _sourceFilter.RegisterValueChangedCallback(evt =>
            {
                var state = evt.newValue;
                
                bool showSheet = state[0];
                bool showLocal = state[1];
                bool showMissing = state[2];

                DataSourceChanged?.Invoke(showSheet, showLocal, showMissing);
            });
        }

        public void ClearFilters()
        {
            _alignDropDown.value = RelicEditorWindow.AlignMode.Default;
            
            var initialState = _sourceFilter.value;
            initialState[0] = true;
            initialState[1] = true;
            initialState[2] = true;
            _sourceFilter.value = initialState;

            _searchField.value = string.Empty;
        }
    }
}