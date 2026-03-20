using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Editor
{
    /// <summary>
    /// 메인 툴바에서 유물 편집기를 여는 버튼을 제공합니다.
    /// </summary>
    public static class RelicEditorToolbar
    {
        [MainToolbarElement("Tools/Open Relic Editor", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement CreateRelicEditorButton()
        {
            var icon = EditorGUIUtility.IconContent("Favorite").image as Texture2D;

            var content = new MainToolbarContent("유물 편집기", icon, "유물 편집기를 엽니다.");

            return new MainToolbarButton(content, OnButtonClicked);
        }

        private static void OnButtonClicked()
        {
            RelicEditorWindow.ShowWindow(); 
        }
    }
}