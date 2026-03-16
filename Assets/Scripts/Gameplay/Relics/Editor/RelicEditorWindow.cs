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
        private VisualElement _rightPanel;
        private RelicList _relicListView;

        [MenuItem("Cardevil/Relic Editor")]
        private static void ShowWindow()
        {
            RelicEditorWindow wnd = GetWindow<RelicEditorWindow>();
            wnd.titleContent = new GUIContent("유물 편집기");
            wnd.minSize = new Vector2(800, 600);
        }

        public void CreateGUI()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/RelicEditorWindow.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            // Data
            _database = LoadRelicDatabase();
            if (!_database) return;
            
            SetupLeftPanel(rootVisualElement);
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

        private void SetupLeftPanel(VisualElement root)
        {
            _relicListView = root.Q<RelicList>("RelicListView");
        }

        private void OnDeleteRelicClicked(string relicId)
        {
            
        }

        private void OnRelicSelectionChanged(Relic selectedRelic)
        {
            
        }
    }
}