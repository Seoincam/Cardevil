using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using Cardevil.Gameplay.Relics.Editor.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor
{
    public class RelicEditorWindow : EditorWindow
    {
        private RelicDatabase _database;
        
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

            _relicList.Setup(_database.relics);
            _relicList.SelectionChanged += OnSelectionChanged;

            _detailView.DataChanged += _relicList.RefreshSelectedRow;
            _detailView.CloseClicked += OnCloseDetail;
            
            _mainToolbar.AddRelicClicked += OnAdd;
            
            ShowDetailPane(false);
        }

        private static RelicDatabase LoadRelicDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:RelicDatabase");
            
            if (guids == null || guids.Length ==0)
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
            newRelic.Initialize("test_id", "새로운 유물");

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
            _relicList.Refresh();
            _detailView.BindRelic(new SerializedObject(newRelic));
        }

        private void OnSelectionChanged(RelicSO selectedRelic)
        {
            ShowDetailPane(true);
            _detailView.BindRelic(new SerializedObject(selectedRelic));
        }

        private void OnDelete(string relicId)
        {
            
        }

        private void OnCloseDetail()
        {
            ShowDetailPane(false);
            _relicList.ClearSelection();
        }

        private void ShowDetailPane(bool isVisible)
        {
            if (_rightPane == null || _detailView == null) return;
            
            _detailView.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            _rightPane.style.flexGrow = isVisible ? 3.5f : 0f;

            _rightPane.style.paddingTop = isVisible ? 10f : 0f;
            _rightPane.style.paddingBottom = isVisible ? 10f : 0f;
            _rightPane.style.paddingLeft = isVisible ? 10f : 0f;
            _rightPane.style.paddingRight = isVisible ? 10f : 0f;
        }
    }
}