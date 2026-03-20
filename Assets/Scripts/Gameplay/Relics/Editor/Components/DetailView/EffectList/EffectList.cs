using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class EffectList : VisualElement
    {
        private SerializedObject _currentRelic;
        private SerializedProperty _effectsProp;

        private readonly ScrollView _effectScrollView;
        private readonly Button _addEffectBtn;
        
        private readonly List<EffectBox> _effectBoxPool = new();
        private static List<(string displayName, Type type)> _cachedEffectTypes;

        public EffectList()
        {
            const string uxmlPath =
                "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/EffectList/EffectList.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(this);

            _effectScrollView = this.Q<ScrollView>("EffectScrollView");
            _addEffectBtn = this.Q<Button>("Add");

            if (_addEffectBtn != null)
                _addEffectBtn.clicked += ShowAddEffectMenu;
                
            CacheEffectTypes();
        }

        private void CacheEffectTypes()
        {
            if (_cachedEffectTypes != null) return;
            
            _cachedEffectTypes = new List<(string, Type)>();
            
            var effectTypes = TypeCache
                .GetTypesDerivedFrom<EffectDefinition>()
                .Where(t => !t.IsAbstract).ToList();

            foreach (Type type in effectTypes)
            {
                string displayName = type.Name;
                try
                {
                    // 단 한 번만 생성하여 이름을 가져오고 캐싱함
                    if (Activator.CreateInstance(type) is EffectDefinition tempInstance)
                    {
                        displayName = tempInstance.EditorName;
                    }
                }
                catch (Exception e)
                {
                    LogEx.LogWarning($"[{type.Name} 임시 인스턴스 생성 실패, 기본 이름을 사용함.");
                }
                
                _cachedEffectTypes.Add((displayName, type));
            }
        }

        public void BindRelic(SerializedObject serializedRelic)
        {
            foreach (var box in _effectBoxPool)
            {
                box.ClearProperties();
            }
            this.Unbind();
            
            _currentRelic = serializedRelic;
            
            if (_currentRelic == null || _currentRelic.targetObject == null)
            {
                _effectsProp = null;
                RefreshEffectListUI();
                return;
            }
            
            SerializedProperty dataProp = _currentRelic.FindProperty("data");
            if (dataProp == null)
            {
                LogEx.LogError("부모인 data 프로퍼티를 찾을 수 없습니다");
                _effectsProp = null;
                return;
            }

            _effectsProp = dataProp.FindPropertyRelative("effects");
            if (_effectsProp == null)
            {
                LogEx.LogError("자식인 effects 프로퍼티를 찾을 수 없습니다");
                return;
            }
            
            RefreshEffectListUI();
        }

        private void RefreshEffectListUI()
        {
            if (_currentRelic == null || _effectsProp == null)
            {
                // 선택된 유물이 없으면 모두 숨김
                foreach (var box in _effectBoxPool)
                {
                    box.style.display = DisplayStyle.None;
                }
                return;
            }

            _currentRelic.Update();
            int targetCount = _effectsProp.arraySize;

            while (_effectBoxPool.Count < targetCount)
            {
                var newBox = new EffectBox();
                newBox.DeleteClicked += OnDeleteClicked;
                _effectBoxPool.Add(newBox);
                _effectScrollView.Add(newBox); // 스크롤 뷰에 추가
            }

            for (int i = 0; i < _effectBoxPool.Count; i++)
            {
                var effectBox = _effectBoxPool[i];

                effectBox.ClearProperties();

                if (i < targetCount)
                {
                    effectBox.style.display = DisplayStyle.Flex;
                    var effectProp = _effectsProp.GetArrayElementAtIndex(i);
                    effectBox.BindEffect(effectProp, i);
                }
                else
                {
                    effectBox.style.display = DisplayStyle.None;
                }
            }
            
            this.Bind(_currentRelic);
        }

        private void ShowAddEffectMenu()
        {
            if (_currentRelic == null) return;

            GenericMenu menu = new();
            
            // 캐싱된 데이터를 사용하여 메뉴 구성
            foreach (var cache in _cachedEffectTypes)
            {
                Type captureType = cache.type; 
                menu.AddItem(new GUIContent(cache.displayName), false, () => AddNewEffect(captureType));
            }

            menu.ShowAsContext();
        }

        private void AddNewEffect(Type effectType)
        {
            if (_currentRelic == null || _effectsProp == null) return;

            _currentRelic.Update(); 
            object newEffect = Activator.CreateInstance(effectType);
            _effectsProp.arraySize++;
            _effectsProp.GetArrayElementAtIndex(_effectsProp.arraySize - 1).managedReferenceValue = newEffect;

            _currentRelic.ApplyModifiedProperties();
            
            this.Unbind();
            RefreshEffectListUI(); 
        }

        private void OnDeleteClicked(int index)
        {
            if (_currentRelic == null || _effectsProp == null) return;

            _currentRelic.Update();
            if (index < 0 || index >= _effectsProp.arraySize) return;
            
            int initialSize = _effectsProp.arraySize;
            
            _effectsProp.DeleteArrayElementAtIndex(index);

            if (_effectsProp.arraySize == initialSize && index < _effectsProp.arraySize)
            {
                _effectsProp.DeleteArrayElementAtIndex(index);
            }

            _currentRelic.ApplyModifiedProperties();
            
            this.Unbind();
            RefreshEffectListUI();
        }
    }
}