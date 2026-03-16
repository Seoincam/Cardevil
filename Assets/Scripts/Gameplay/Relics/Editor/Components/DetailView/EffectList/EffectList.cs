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

        public void BindRelic(SerializedObject serializedRelic)
        {
            _currentRelic = serializedRelic;
            _effectsProp = serializedRelic.FindProperty("effects");
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
                .GetTypesDerivedFrom<EffectBase>()
                .Where(t => !t.IsAbstract).ToList();

            GenericMenu menu = new();
            foreach (Type type in effectTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => AddNewEffect(type));
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

            var targetProp = _effectsProp.GetArrayElementAtIndex(index);

            // 리스트 요소가 객체인 경우, 삭제 명령은 해당 객체를 null로 만들기만 함.
            // 따라서, 해당 칸 자체를 날리기 위해선 두 번 호출해야함.
            if (targetProp.managedReferenceValue != null)
            {
                _effectsProp.DeleteArrayElementAtIndex(index);
            }
            _effectsProp.DeleteArrayElementAtIndex(index);

            _currentRelic.ApplyModifiedProperties();
            RefreshEffectListUI();
        }
    }
}