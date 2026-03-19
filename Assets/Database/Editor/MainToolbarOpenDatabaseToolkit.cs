using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Database
{
    /// <summary>
    /// 메인 툴바에서 데이터베이스 툴킷 창을 여는 버튼을 제공합니다.
    /// </summary>
    public static class MainToolbarOpenDatabaseToolkit
    {
        [MainToolbarElement("Tools/OpenDatabaseToolkit", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarButton OpenDatabaseToolkitButton()
        {
            var icon = EditorGUIUtility.IconContent("d_ScriptableObject Icon").image as Texture2D;
            var content = new MainToolbarContent("DB 열기", icon, "데이터베이스 툴킷 창을 엽니다.");
            return new MainToolbarButton(content, OnButtonClick);
        }

        private static void OnButtonClick()
        {
            DBInitializerSO initializer = Selection.activeObject as DBInitializerSO;
            DBInitializerWindow.OpenWindow(initializer);
        }
    }
}
