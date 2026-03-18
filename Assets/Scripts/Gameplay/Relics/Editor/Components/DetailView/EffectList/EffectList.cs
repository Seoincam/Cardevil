using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using System.Linq;
using UnityEditor;
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
        }

        public void BindRelic(RelicSO relic)
        {
            var serializedRelic = new SerializedObject(relic);
            _currentRelic = serializedRelic;
            
            SerializedProperty dataProp = serializedRelic.FindProperty("data");
            if (dataProp == null)
            {
                LogEx.LogError("부모인 data 프로퍼티를 찾을 수 없습니다");
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
            if (_currentRelic == null) return;

            foreach (var child in _effectScrollView.Children())
            {
                if (child is EffectBox effectBox)
                    effectBox.DeleteClicked = null;
            }

            _effectScrollView.Clear();

            for (int i = 0; i < _effectsProp.arraySize; i++)
            {
                var effectProp = _effectsProp.GetArrayElementAtIndex(i);

                var effectBox = new EffectBox(effectProp, i);
                effectBox.DeleteClicked += OnDeleteClicked;

                _effectScrollView.Add(effectBox);
            }
        }

        private void ShowAddEffectMenu()
        {
            if (_currentRelic == null) return;

            var effectTypes = TypeCache
                .GetTypesDerivedFrom<EffectDefinition>()
                .Where(t => !t.IsAbstract).ToList();

            GenericMenu menu = new();
            foreach (Type type in effectTypes)
            {
                string displayName = type.Name;

                try
                {
                    if (Activator.CreateInstance(type) is EffectDefinition tempInstance)
                    {
                        displayName = tempInstance.EditorName;
                    }
                }
                catch (Exception e)
                {
                    LogEx.LogWarning($"[{type.Name} 임시 인스턴스 생성 실패, 기본 이름을 사용함.");
                }
                
                menu.AddItem(new GUIContent(displayName), false, () => AddNewEffect(type));
            }

            menu.ShowAsContext();
        }

        private void AddNewEffect(Type effectType)
        {
            object newEffect = Activator.CreateInstance(effectType);
            _effectsProp.arraySize++;
            _effectsProp.GetArrayElementAtIndex(_effectsProp.arraySize - 1).managedReferenceValue = newEffect;

            _currentRelic.ApplyModifiedProperties();
            RefreshEffectListUI();
        }

        private void OnDeleteClicked(int index)
        {
            if (_currentRelic == null || _effectsProp == null) return;

            if (index < 0 || index >= _effectsProp.arraySize) return;
            
            int initialSize = _effectsProp.arraySize;
            
            _effectsProp.DeleteArrayElementAtIndex(index);

            if (_effectsProp.arraySize == initialSize && index < _effectsProp.arraySize)
            {
                // 리스트 요소가 객체인 경우, 삭제 명령은 해당 객체를 null로 만들기만 함.
                // 따라서, 해당 칸 자체를 날리기 위해선 두 번 호출해야함.
                _effectsProp.DeleteArrayElementAtIndex(index);
            }

            _currentRelic.ApplyModifiedProperties();
            RefreshEffectListUI();
        }
    }
}