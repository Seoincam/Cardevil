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
        
        private readonly Button _addRelicButton;
        
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

            if (_addRelicButton != null)
                _addRelicButton.clicked += () => AddRelicClicked?.Invoke();
        }
    }
}