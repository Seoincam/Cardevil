using Cardevil.Utils;
using System;
using UnityEditor;
using UnityEngine;

namespace Cardevil.Editors
{
    /// <summary>
    /// <see cref="VisibleOnly"/> attribute drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(TileVector))]
    public class TileVectorDrawer : PropertyDrawer
    {
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 한줄
            return EditorGUIUtility.singleLineHeight; 
        }
        private GUIContent _iContent = new GUIContent("i");
        private GUIContent _jContent = new GUIContent("j");
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Vector2int처럼 그리기
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginProperty(position, label, property);
            

            SerializedProperty iProperty = property.FindPropertyRelative("_i");
            SerializedProperty jProperty = property.FindPropertyRelative("_j");
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