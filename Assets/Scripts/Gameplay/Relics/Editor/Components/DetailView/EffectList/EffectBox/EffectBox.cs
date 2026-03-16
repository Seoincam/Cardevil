using Cardevil.Core.Utils;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class EffectBox : VisualElement
    {
        public Action<int> DeleteClicked;
        private int _effectIndex;
        
        // Effect
        private readonly Label _nameLabel;
        private readonly Label _descLabel;
        private readonly VisualElement _propertiesContainer;
        
        // Management
        private readonly Button _deleteButton;

        public EffectBox(SerializedProperty effectProp, int index) : this()
        {
            BindEffect(effectProp, index);
        }
        
        public EffectBox()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/EffectList/EffectBox/EffectBox.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"$UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }
            
            visualTree.CloneTree(this);
            
            _nameLabel = this.Q<Label>("EffectName");
            _descLabel = this.Q<Label>("Description");
            _propertiesContainer = this.Q("PropertiesContainer");
            _deleteButton = this.Q<Button>("DeleteButton");
            
            if (_deleteButton != null)
                _deleteButton.clicked += () => DeleteClicked?.Invoke(_effectIndex);
        }
        
        public void BindEffect(SerializedProperty effectProp, int index)
        {
            _effectIndex = index;
            
            _propertiesContainer.Clear();

            SerializedProperty iterator = effectProp.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;

                PropertyField field = new(iterator);
                field.BindProperty(iterator);
                _propertiesContainer.Add(field);
            }

            object actualEffect = effectProp.managedReferenceValue;
            if (actualEffect != null)
            {
                _nameLabel.text = actualEffect.GetType().Name;
                _descLabel.text = "아직 설명 관련한 것은 추가하지 않았음.";
            }
        }
    }
}