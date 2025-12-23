using UnityEngine;
using UnityEditor;

namespace Cardevil.Save.Editor
{
    [CustomPropertyDrawer(typeof(SaveLoadManager))]
    public class SaveLoadManagerDrawer : PropertyDrawer
    {
        const float k_VerticalSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Foldout 헤더
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            position.y += position.height + k_VerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var spDefaultSaveName = property.FindPropertyRelative("DefaultSaveName");
                var spExceptionScenes = property.FindPropertyRelative("exceptionScenes");
                var spCurrentSave     = property.FindPropertyRelative("_currentSave");

                // DefaultSaveName
                var h0 = EditorGUI.GetPropertyHeight(spDefaultSaveName, true);
                position.height = h0;
                EditorGUI.PropertyField(position, spDefaultSaveName, new GUIContent("Default Save Name"), true);
                position.y += h0 + k_VerticalSpacing;

                // Exception Scenes (List)
                var h1 = EditorGUI.GetPropertyHeight(spExceptionScenes, true);
                position.height = h1;
                EditorGUI.PropertyField(position, spExceptionScenes, new GUIContent("Exception Scenes"), true);
                position.y += h1 + k_VerticalSpacing;

                // Current Save (Nested class)
                var h2 = EditorGUI.GetPropertyHeight(spCurrentSave, true);
                position.height = h2;
                EditorGUI.PropertyField(position, spCurrentSave, new GUIContent("Current Save"), true);
                position.y += h2 + k_VerticalSpacing;

                // 버튼 영역
                float line = EditorGUIUtility.singleLineHeight;
                var full = new Rect(position.x, position.y, position.width, line);
                var left = new Rect(position.x, position.y, (position.width - 4) * 0.5f, line);
                var right = new Rect(left.xMax + 4, position.y, left.width, line);

                // 대상 오브젝트 안전 획득 (멀티셀렉션/리스트 내 중첩 케이스 대비)
                object target = GetTargetObjectOfProperty(property);
                var manager = target as SaveLoadManager;

                using (new EditorGUI.DisabledScope(manager == null))
                {
                    if (GUI.Button(left, "Save"))
                    {
                        // 플레이 모드에서만 동작하게 하려면 EditorApplication.isPlaying 체크 추천
                        manager?.SaveGame();
                    }
                    if (GUI.Button(right, "Load"))
                    {
                        manager?.ReloadGame();
                    }

                    position.y += line + k_VerticalSpacing;

                    // Delete 버튼 (현재 세이브 이름 필요)
                    string currentName = manager?.CurrentSave?.Name ?? spCurrentSave.FindPropertyRelative("Name")?.stringValue;
                    if (GUI.Button(new Rect(position.x, position.y, position.width, line), "Delete"))
                    {
                        if (!string.IsNullOrEmpty(currentName))
                            manager?.DeleteSave(currentName);
                        else
                            Debug.LogWarning("No save name to delete.");
                    }
                    position.y += line + k_VerticalSpacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float total = EditorGUIUtility.singleLineHeight + k_VerticalSpacing; // foldout 헤더

            if (!property.isExpanded)
                return total;

            var spDefaultSaveName = property.FindPropertyRelative("DefaultSaveName");
            var spExceptionScenes = property.FindPropertyRelative("exceptionScenes");
            var spCurrentSave     = property.FindPropertyRelative("_currentSave");

            total += EditorGUI.GetPropertyHeight(spDefaultSaveName, true) + k_VerticalSpacing;
            total += EditorGUI.GetPropertyHeight(spExceptionScenes, true) + k_VerticalSpacing;
            total += EditorGUI.GetPropertyHeight(spCurrentSave, true) + k_VerticalSpacing;

            // 버튼 2줄
            total += EditorGUIUtility.singleLineHeight + k_VerticalSpacing; // Save/Load
            total += EditorGUIUtility.singleLineHeight + k_VerticalSpacing; // Delete

            return total;
        }

        // UnityEditor.PropertyDrawer에서 안전하게 실제 타깃 객체를 얻는 유틸
        static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var target = prop.serializedObject.targetObject;
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = target;

            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        static object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();
            while (type != null)
            {
                var f = type.GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (f != null) return f.GetValue(source);
                var p = type.GetProperty(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(source, null);
                type = type.BaseType;
            }
            return null;
        }

        static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
                if (!enm.MoveNext()) return null;
            return enm.Current;
        }
    }
}
