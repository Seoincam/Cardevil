using Cardevil.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace Cardevil.Editors
{
    /// <summary>
    /// <see cref="VisibleOnly"/> attribute drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(TileVectorDrawer))]
    public class TileVectorDrawer : PropertyDrawer
    {
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        private GUIContent _iContent = new GUIContent("i");
        private GUIContent _jContent = new GUIContent("j");
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Vector2int처럼 그리기
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty iProperty = property.FindPropertyRelative("i");
            SerializedProperty jProperty = property.FindPropertyRelative("j");
            float width = position.width / 2f;
            Rect iRect = new Rect(position.x, position.y, width, position.height);
            Rect jRect = new Rect(position.x + width, position.y, width, position.height);
            int newI = EditorGUI.IntField(iRect, _iContent, iProperty.intValue);
            int newJ = EditorGUI.IntField(jRect, _jContent, jProperty.intValue);
            if (newI != iProperty.intValue || newJ != jProperty.intValue)
            {
                iProperty.intValue = newI;
                jProperty.intValue = newJ;
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}