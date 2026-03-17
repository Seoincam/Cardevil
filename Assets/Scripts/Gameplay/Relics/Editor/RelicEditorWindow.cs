using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using Cardevil.Gameplay.Relics.Editor.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor
{
    public class RelicEditorWindow : EditorWindow
    {
        // Data
        private RelicDatabase _database;
        private RelicSO _selectedRelic;

        // Filtering
        private List<RelicSO> _displayRelics = new();
        private AlignMode _currentAlign = AlignMode.Default;
        private string _currentSearchKeyword;

        // UI View
        private MainToolbar _mainToolbar;
        private RelicList _relicList;
        private DetailView _detailView;
        private VisualElement _rightPane;

        private const string RelicFolderPath = "Assets/Resources/ScriptableObjects/Relics";

        [MenuItem("Cardevil/Relic Editor")]
        private static void ShowWindow()
        {
            RelicEditorWindow wnd = GetWindow<RelicEditorWindow>();
            wnd.titleContent = new GUIContent("유물 편집기");
            wnd.minSize = new Vector2(1300, 920);
        }

        public enum AlignMode
        {
            Default,
            Id,
            Name
        }

        public void CreateGUI()
        {
            // Data
            _database = LoadRelicDatabase();
            if (!_database) return;

            // UI
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/RelicEditorWindow.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            _mainToolbar = rootVisualElement.Q<MainToolbar>();
            _relicList = rootVisualElement.Q<RelicList>();
            _detailView = rootVisualElement.Q<DetailView>();

            _rightPane = rootVisualElement.Q<VisualElement>("RightPane");

            RefreshDisplayList();
            _relicList.Setup(_displayRelics);
            _relicList.SelectionChanged += OnSelectionChanged;
            _relicList.DeleteClicked += OnDelete;

            _detailView.DataChanged += _relicList.RefreshSelectedRow;
            _detailView.CloseClicked += OnCloseDetail;

            _mainToolbar.AddRelicClicked += OnAdd;
            _mainToolbar.AlignChanged += OnAlignModeChanged;
            _mainToolbar.KeywordChanged += OnKeywordChanged;
            
            OnCloseDetail();
        }

        public void DestroyGUI()
        {

            ShowDetailPane(false);
        }

        private static RelicDatabase LoadRelicDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:RelicDatabase");

            if (guids == null || guids.Length == 0)
            {
                LogEx.LogError("RelicDatabase Asset이 존재하지 않음. 추가해야함.");
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<RelicDatabase>(path);
        }

        private void OnAdd()
        {
            if (!_database) return;

            // 생성
            RelicSO newRelic = CreateInstance<RelicSO>();
            newRelic.Initialize("test_id", "새로운 유물", true);

            if (!AssetDatabase.IsValidFolder(RelicFolderPath))
            {
                LogEx.LogError($"유물 폴더가 존재하지 않습니다. 폴더를 만들어주세요. path: {RelicFolderPath}");
                return;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{RelicFolderPath}/Relic.asset");
            AssetDatabase.CreateAsset(newRelic, assetPath);

            SerializedObject serializedDb = new(_database);
            SerializedProperty relicsProp = serializedDb.FindProperty("relics");

            if (relicsProp == null)
            {
                LogEx.LogError("Database에서 'relics' 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            relicsProp.arraySize++;
            int newIndex = relicsProp.arraySize - 1;

            relicsProp.GetArrayElementAtIndex(newIndex).objectReferenceValue = newRelic;
            serializedDb.ApplyModifiedProperties();

            // 실제 저장
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // UI 갱신
            RefreshDisplayList();
            _detailView.BindRelic(newRelic);
        }

        private void OnSelectionChanged(RelicSO selectedRelic)
        {
            _selectedRelic = selectedRelic;

            ShowDetailPane(true);
            _detailView.BindRelic(selectedRelic);
        }

        private void OnDelete(string relicId)
        {
            if (!_database || _database.relics == null) return;

            RelicSO targetRelic = _database.relics.FirstOrDefault(r => r && r.Data.Id == relicId);
            if (!targetRelic)
            {
                LogEx.LogError($"삭제가 요청된 유물을 찾을 수 없습니다. id: {relicId}");
                return;
            }

            bool isConfirm = EditorUtility.DisplayDialog(
                "유물 삭제",
                $"정말 {targetRelic.Data.DisplayName}({relicId})를 삭제하시겠습니까?",
                "삭제",
                "취소"
            );

            if (!isConfirm) return;

            // DB에서 실제 제거
            _database.relics.Remove(targetRelic);
            EditorUtility.SetDirty(_database);

            // 실제 에셋 제거
            string assetPath = AssetDatabase.GetAssetPath(targetRelic);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // UI 갱신
            RefreshDisplayList();            
            OnCloseDetail();

            if (_selectedRelic)
            {
                OnSelectionChanged(_selectedRelic);
            }
        }

        private void OnCloseDetail()
        {
            ShowDetailPane(false);
            _relicList.ClearSelection();
        }

        private void OnKeywordChanged(string keyword)
        {
            _currentSearchKeyword = keyword;
            RefreshDisplayList();
        }

        private void OnAlignModeChanged(AlignMode mode)
        {
            _currentAlign = mode;
            RefreshDisplayList();
        }

        private void ShowDetailPane(bool isVisible)
        {
            if (_rightPane == null || _detailView == null) return;

            _detailView.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            _rightPane.style.flexGrow = isVisible ? 3.5f : 2f;

            _rightPane.style.paddingTop = isVisible ? 10f : 0f;
            _rightPane.style.paddingBottom = isVisible ? 10f : 0f;
            _rightPane.style.paddingLeft = isVisible ? 10f : 0f;
            _rightPane.style.paddingRight = isVisible ? 10f : 0f;
        }

        private void RefreshDisplayList()
        {
            if (!_database || _database.relics == null) return;

            // 검색 필터링
            var filtered = _database.relics.Where(r =>
                r &&
                (string.IsNullOrEmpty(_currentSearchKeyword) ||
                 r.Data.DisplayName.Contains(_currentSearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                 r.Data.Id.Contains(_currentSearchKeyword, StringComparison.OrdinalIgnoreCase))
            );

            // 정렬
            var sorted = _currentAlign switch
            {
                AlignMode.Id => filtered.OrderBy(r => r.Data.Id),
                AlignMode.Name => filtered.OrderBy(r => r.Data.DisplayName),
                _ => filtered
            };

            _displayRelics = sorted.ToList();

            // UI 갱신
            _relicList.UpdateSource(_displayRelics);
            if (_selectedRelic)
            {
                _relicList.SelectItemByObject(_selectedRelic);
            }
        }
    }
}
