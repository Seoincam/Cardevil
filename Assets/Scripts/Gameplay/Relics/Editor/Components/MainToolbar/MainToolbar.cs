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
        public event Action<RelicEditorWindow.AlignMode> AlignChanged;
        public event Action<string> KeywordChanged;
        
        private readonly Button _addRelicButton;
        private readonly EnumField _alignDropDown;
        private readonly TextField _searchField;
        
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
            _alignDropDown = this.Q<EnumField>("AlignDropDown");
            _searchField = this.Q<TextField>("SearchField");
            
            _addRelicButton.clicked += () => AddRelicClicked?.Invoke();
            _alignDropDown.RegisterValueChangedCallback(evt => AlignChanged?.Invoke((RelicEditorWindow.AlignMode)evt.newValue));
            _searchField.RegisterValueChangedCallback(evt => KeywordChanged?.Invoke(evt.newValue.ToString()));
        }
    }
}