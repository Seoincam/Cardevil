using Cardevil.Gameplay.Relics.Effects.ScoreEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor
{
    [CustomPropertyDrawer(typeof(IValueResolver), true)]
    public class ValueResolverDrawer : PropertyDrawer
    {
        private static List<Type> _resolverTypes;
        private static List<string> _typeNames;
        private string _lastTypeFullName = string.Empty;
    
        private static void InitializeTypes()
        {
            if (_resolverTypes != null) return;
            _resolverTypes = TypeCache.GetTypesDerivedFrom<IValueResolver>()
                .Where(t => !t.IsAbstract && !t.IsInterface).ToList();
            _typeNames = _resolverTypes.Select(t => t.Name).ToList();
        }
    
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            InitializeTypes();
            var container = new VisualElement();
            
            container.style.paddingLeft = 5;
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = new UnityEngine.Color(1f, 0.84f, 0f, 0.3f);
    
            int currentIndex = GetCurrentIndex(property);
            var dropdown = new DropdownField(property.displayName, _typeNames, currentIndex);
            
            var fieldsContainer = new VisualElement();
            fieldsContainer.style.paddingLeft = 10;
    
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = _typeNames.IndexOf(evt.newValue);
                if (index >= 0)
                {
                    property.managedReferenceValue = Activator.CreateInstance(_resolverTypes[index]);
                    property.serializedObject.ApplyModifiedProperties();
                    RebuildFields(property, fieldsContainer);
                }
            });
            
            container.Add(dropdown);
            container.Add(fieldsContainer);
            
            container.TrackPropertyValue(property, (prop) => 
            {
                string currentType = prop.managedReferenceFullTypename;
                if (_lastTypeFullName != currentType)
                {
                    _lastTypeFullName = currentType;
                    RebuildFields(prop, fieldsContainer);
                }
            });
    
            _lastTypeFullName = property.managedReferenceFullTypename;
            RebuildFields(property, fieldsContainer);
    
            return container;
        }
    
        private void RebuildFields(SerializedProperty property, VisualElement container)
        {
            container.Clear();
            if (property.managedReferenceValue == null) return;
    
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();
            bool enterChildren = true;
    
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                
                PropertyField field = new(iterator);
                field.BindProperty(iterator);
                container.Add(field);
            }
        }
    
        private int GetCurrentIndex(SerializedProperty property)
        {
            string typeNameString = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(typeNameString)) return 0;
            string currentTypeName = typeNameString.Split(' ').Last();
            return _resolverTypes.FindIndex(t => t.FullName == currentTypeName);
        }
    }
}