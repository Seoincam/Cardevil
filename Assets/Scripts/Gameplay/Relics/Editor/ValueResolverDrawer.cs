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
            
            // 스타일 설정
            container.style.paddingLeft = 5;
            container.style.borderLeftWidth = 2;
            container.style.borderLeftColor = new UnityEngine.Color(1f, 0.84f, 0f, 0.3f);
    
            // 1. 드롭다운 설정
            int currentIndex = GetCurrentIndex(property);
            var dropdown = new DropdownField(property.displayName, _typeNames, currentIndex);
            
            // 2. 하위 필드를 담을 컨테이너
            var fieldsContainer = new VisualElement();
            fieldsContainer.style.paddingLeft = 10;
    
            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = _typeNames.IndexOf(evt.newValue);
                if (index >= 0)
                {
                    property.managedReferenceValue = Activator.CreateInstance(_resolverTypes[index]);
                    property.serializedObject.ApplyModifiedProperties();
                    // 🌟 타입이 바뀌었으므로 즉시 재빌드
                    RebuildFields(property, fieldsContainer);
                }
            });
            
            container.Add(dropdown);
            container.Add(fieldsContainer);
    
            // 🌟 최적화 2: 지능형 추적 (TrackPropertyValue 개선)
            // 수치가 바뀔 때가 아니라, '타입 정보'가 바뀌었을 때만 리빌드하도록 체크
            container.TrackPropertyValue(property, (prop) => 
            {
                string currentType = prop.managedReferenceFullTypename;
                if (_lastTypeFullName != currentType)
                {
                    _lastTypeFullName = currentType;
                    RebuildFields(prop, fieldsContainer);
                }
            });
    
            // 초기화 시 1회 빌드
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
                // 🌟 PropertyField는 스스로 데이터 동기화를 하기 때문에 
                // '타입'이 바뀔 때 한 번만 생성해두면 수치 변경 시에는 다시 만들 필요가 없습니다.
                PropertyField field = new PropertyField(iterator);
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