using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using Cardevil.Gameplay.Relics.Editor.Components;
using Cardevil.Gameplay.Relics.Editor.Components.Sync;
using Database;
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
        private DBInitializerSO _sheetDatabaseInitializer;
        private RelicDatabase _database;
        private RelicSyncService _syncService;
        
        private RelicSO _selectedRelic;

        // Filtering
        private List<RelicSO> _displayRelics = new();
        private AlignMode _currentAlign = AlignMode.Default;
        private string _currentSearchKeyword;
        private (bool showSheet, bool showLocal, bool showMissing) _sourceFilters = new(true, true, true);
        
        // UI View
        private VisualElement _rootContainer;
        private MainToolbar _mainToolbar;
        private RelicList _relicList;
        private DetailView _detailView;
        private VisualElement _rightPane;

        private VisualElement _helpBoxContainer;

        private const string RelicSavePath = "Assets/Resources/ScriptableObjects/Relics";

        [MenuItem("Cardevil/Relic Editor")]
        public static void ShowWindow()
        {
            RelicEditorWindow wnd = GetWindow<RelicEditorWindow>();
            wnd.titleContent = new GUIContent("유물 편집기");
            wnd.minSize = new Vector2(1600, 920);
            wnd.Show();
        }

        public static void OpenRelicSO(RelicSO relicSo)
        {
            RelicEditorWindow wnd = GetWindow<RelicEditorWindow>();
            wnd.titleContent = new GUIContent("유물 편집기");
            wnd.minSize = new Vector2(1600, 920);

            if (wnd._relicList != null)
            {
                wnd._selectedRelic = relicSo;
                wnd.ApplyInitialSelection();
            }
            
            wnd.Show();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
        }

        public enum AlignMode
        {
            Default,
            Id,
            Name,
            Rarity,
            SourceType
        }

        public void CreateGUI()
        {
            _database = LoadRelicDatabase();
            if (!_database) return;

            _syncService = new RelicSyncService(_database);

            // UI
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/RelicEditorWindow.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(rootVisualElement);
            _rootContainer = rootVisualElement.Q<VisualElement>("RootContainer");
            _mainToolbar = rootVisualElement.Q<MainToolbar>();
            _relicList = rootVisualElement.Q<RelicList>();
            _detailView = rootVisualElement.Q<DetailView>();

            _rightPane = rootVisualElement.Q<VisualElement>("RightPane");
            _helpBoxContainer = rootVisualElement.Q<VisualElement>("HelpBoxContainer");

            RefreshDisplayList();
            _relicList.Setup(_displayRelics);
            _relicList.SelectionChanged += OnSelectionChanged;
            _relicList.DeleteClicked += OnDelete;

            _detailView.DataChanged += _relicList.RefreshSelectedRow;
            _detailView.CloseClicked += OnCloseDetail;
            _detailView.DeleteClicked += OnDelete;
            
            _mainToolbar.AddRelicClicked += OnAdd;
            _mainToolbar.DownloadClicked += OnDownload;
            _mainToolbar.SyncClicked += OnSync;
            _mainToolbar.AlignChanged += OnAlignModeChanged;
            _mainToolbar.KeywordChanged += OnKeywordChanged;
            _mainToolbar.DataSourceChanged += (showSheet, showLocal, showMissing) =>
            {
                _sourceFilters = (showSheet, showLocal, showMissing);
                RefreshDisplayList();
            };

            if (_selectedRelic)
            {
                ApplyInitialSelection();
            }
            else
            {
                OnCloseDetail();   
            }
            
            HandlePlayModeChanged(EditorApplication.isPlaying);
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

        private void OnDownload()
        {
            if (!_sheetDatabaseInitializer)
            {
                string[] guids = AssetDatabase.FindAssets("t:DBInitializerSO");

                if (guids == null || guids.Length == 0)
                {
                    LogEx.LogError("DBInitializerSO를 찾지 못함.");
                    return;
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _sheetDatabaseInitializer = AssetDatabase.LoadAssetAtPath<DBInitializerSO>(path);
            }
            
            _sheetDatabaseInitializer.DownloadGoogleSheet();
        }
        
        private void OnSync()
        {
            var result = _syncService.ExecuteSync();
            var popup = new SyncSummaryPopup(result);
            rootVisualElement.Add(popup);

            if (result.HasAnyChange)
            {
                RefreshDisplayList();
            }
        }

        private void ApplyInitialSelection()
        {
            _mainToolbar.ClearFilters();
            _relicList.SelectItemByObject(_selectedRelic);
            OnSelectionChanged(_selectedRelic);
        }

        private void OnAdd()
        {
            if (!_database) return;

            // 생성
            RelicSO newRelic = CreateInstance<RelicSO>();
            newRelic.InitializeFromLocal(GenerateLocalId(_database), "새 로컬 유물");

            if (!AssetDatabase.IsValidFolder(RelicSavePath))
            {
                LogEx.LogError($"유물 폴더가 존재하지 않습니다. 폴더를 만들어주세요. path: {RelicSavePath}");
                return;
            }

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{RelicSavePath}/Relic.asset");
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
            _relicList.SelectItemByObject(newRelic);
            _detailView.BindRelic(newRelic);
            ShowDetailPane(true);
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
                _relicList.SelectItemByObject(_selectedRelic);
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

            _rightPane.style.flexGrow = isVisible ? 5f : 2.75f;

            _rightPane.style.paddingTop = isVisible ? 10f : 0f;
            _rightPane.style.paddingBottom = isVisible ? 10f : 0f;
            _rightPane.style.paddingLeft = isVisible ? 10f : 0f;
            _rightPane.style.paddingRight = isVisible ? 10f : 0f;
        }

        private void RefreshDisplayList()
        {
            if (!_database || _database.relics == null) return;

            var (showSheet, showLocal, showMissing) = _sourceFilters;
            var filtered = _database.relics.Where(r =>
                r && 
    
                // Source 
                ((showSheet && r.FromSheet) || 
                 (showLocal && r.FromLocal) || 
                 (showMissing && r.IsMissing)) &&
     
                // 검색
                (string.IsNullOrEmpty(_currentSearchKeyword) ||
                 r.Data.DisplayName.Contains(_currentSearchKeyword, StringComparison.OrdinalIgnoreCase) ||
                 r.Data.Id.Contains(_currentSearchKeyword, StringComparison.OrdinalIgnoreCase))
            );

            // 정렬
            var sorted = _currentAlign switch
            {
                AlignMode.Id => filtered.OrderBy(r => r.Data.Id),
                AlignMode.Name => filtered.OrderBy(r => r.Data.DisplayName),
                AlignMode.Rarity => filtered.OrderBy(r => r.Data.Rarity),
                AlignMode.SourceType => filtered.OrderBy(r => r.Source),
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

        private void HandlePlayModeChanged(PlayModeStateChange state)
        {
            bool isPlaying = EditorApplication.isPlaying || state == PlayModeStateChange.ExitingEditMode;
            HandlePlayModeChanged(isPlaying);
        }

        private void HandlePlayModeChanged(bool isPlaying)
        {
            _helpBoxContainer.Clear();
            if (isPlaying)
            {
                _rootContainer.SetEnabled(false);
                _helpBoxContainer.Add(
                    new HelpBox("플레이 모드에선 유물을 편집할 수 없습니다.", HelpBoxMessageType.Warning)
                );
            }
            else
            {
                _rootContainer.SetEnabled(true);
            }
        }

        private static string GenerateLocalId(RelicDatabase database)
        {
            string timeStamp = DateTime.Now.ToString("yyMMdd-HH-mm-ss");
            string candidateId = $"local-{timeStamp}";

            int safetyIndex = 0;
            string finalId = candidateId;

            while (database.relics.Any(r => r.Data.Id == finalId))
            {
                safetyIndex++;
                finalId = $"{finalId}-{safetyIndex}";
            }

            return finalId;
        }
    }
}
