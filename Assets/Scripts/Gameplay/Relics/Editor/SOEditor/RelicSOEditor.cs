using Cardevil.Gameplay.Relics.Core;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.SOEditor
{
    [CustomEditor(typeof(RelicSO))]
    public class RelicSOEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();

            HelpBox helpBox = new("유물은 '유물 편집기'를 통해서만 수정할 수 있습니다.", HelpBoxMessageType.Warning);
            root.Add(helpBox);

            Button openEditorButton = new(() =>
            {
                RelicEditorWindow.OpenRelicSO(target as RelicSO);
            }) { text = "유물 편집기에서 열기" };
            root.Add(openEditorButton);
            
            return root;
        }
    }
}