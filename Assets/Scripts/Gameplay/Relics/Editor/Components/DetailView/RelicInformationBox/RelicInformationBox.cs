using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class RelicInformationBox : VisualElement
    {
        public event Action DataChanged;
        
        // 미리보기
        private readonly Image _iconPreview;
        private readonly Label _titlePreview;
        private readonly Label _descPreview;
        
        // 입력 필드
        private readonly TextField _idField;
        private readonly ObjectField _iconField;
        private readonly TextField _nameField;
        private readonly TextField _descField;
        
        public RelicInformationBox()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/RelicInformationBox/RelicInformationBox.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(this);
            
            _iconPreview = this.Q<Image>("IconPreview");
            _titlePreview = this.Q<Label>("TitlePreview");
            _descPreview = this.Q<Label>("DescriptionPreview");
            
            _idField = this.Q<TextField>("IdField");
            _iconField = this.Q<ObjectField>("IconField");
            _nameField = this.Q<TextField>("NameField");
            _descField = this.Q<TextField>("DescriptionField");
            
            _iconField.RegisterValueChangedCallback(evt => _iconPreview.sprite = evt.newValue as Sprite);
            _nameField.RegisterValueChangedCallback(evt => _titlePreview.text = evt.newValue);
            _descField.RegisterValueChangedCallback(evt => _descPreview.text = evt.newValue);
            
            _idField.RegisterValueChangedCallback(_ => DataChanged?.Invoke());
            _iconField.RegisterValueChangedCallback(_ => DataChanged?.Invoke());
            _nameField.RegisterValueChangedCallback(_ => DataChanged?.Invoke());
            _descField.RegisterValueChangedCallback(_ => DataChanged?.Invoke());
        }

        public void BindRelic(SerializedObject serializedRelic)
        {
            this.Bind(serializedRelic);
            LogEx.Log((serializedRelic.targetObject as RelicSO)!.Data.DisplayName);
        }
    }
}