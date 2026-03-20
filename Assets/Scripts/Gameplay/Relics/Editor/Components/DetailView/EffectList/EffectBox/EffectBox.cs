using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
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
        private static VisualTreeAsset _cachedAsset;
    
        private string _boundPropertyPath;
        private SerializedObject _boundSerializedObject;
        private EventCallback<SerializedPropertyChangeEvent> _changeEventCallback;
    
        public EffectBox(SerializedProperty effectProp, int index) : this()
        {
            BindEffect(effectProp, index);
        }
        
        public EffectBox()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/EffectList/EffectBox/EffectBox.uxml";
    
            if (!_cachedAsset)
            {
                _cachedAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            }
    
            if (!_cachedAsset)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }
            
            _cachedAsset.CloneTree(this);
            
            _nameLabel = this.Q<Label>("EffectName");
            _descLabel = this.Q<Label>("Description");
            _propertiesContainer = this.Q("PropertiesContainer");
            _deleteButton = this.Q<Button>("DeleteButton");
            
            if (_deleteButton != null)
                _deleteButton.clicked += () => DeleteClicked?.Invoke(_effectIndex);
    
            _changeEventCallback = OnPropertyChanged;
        }
        
        public void BindEffect(SerializedProperty effectProp, int index)
        {
            _effectIndex = index;
            
            ClearProperties();
    
            if (effectProp == null || effectProp.serializedObject == null)
            {
                _nameLabel.text = "Invalid Effect";
                _descLabel.text = "유효하지 않은 이펙트입니다.";
                return;
            }
    
            _boundSerializedObject = effectProp.serializedObject;
            _boundPropertyPath = effectProp.propertyPath;
    
            this.Bind(_boundSerializedObject);
    
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
            
            _propertiesContainer.RegisterCallback(_changeEventCallback);
    
            UpdateLabels(effectProp);
        }
    
        public void ClearProperties()
        {
            if (_changeEventCallback != null)
            {
                _propertiesContainer.UnregisterCallback(_changeEventCallback);
            }
    
            _propertiesContainer.Unbind();
            _propertiesContainer.Clear();
            this.Unbind();
            
            _nameLabel.text = "";
            _descLabel.text = "";
            _boundSerializedObject = null;
            _boundPropertyPath = null;
        }
    
        private void OnPropertyChanged(SerializedPropertyChangeEvent evt)
        {
            if (_boundSerializedObject == null) return;
            
            _boundSerializedObject.UpdateIfRequiredOrScript();
            SerializedProperty freshProp = _boundSerializedObject.FindProperty(_boundPropertyPath);
    
            if (freshProp != null)
            {
                UpdateLabels(freshProp);
            }
        }
    
        private void UpdateLabels(SerializedProperty effectProp)
        {
            if (effectProp == null || effectProp.serializedObject == null) return;
            
            object actualEffect = effectProp.managedReferenceValue;
            if (actualEffect is EffectDefinition effectDef)
            {
                int slashIndex = effectDef.EditorName.LastIndexOf('/');
                var trimmedName = slashIndex < 0 
                    ? effectDef.EditorName 
                    : effectDef.EditorName[(slashIndex + 1)..];
                
                _nameLabel.text = trimmedName;
                _descLabel.text = effectDef.EditorDescription;
            }
            else
            {
                _nameLabel.text = "Empty Effect";
                _descLabel.text = "이펙트가 할당되지 않았습니다.";
            }
        }
    }
}