// using Cardevil.Utils;
// using System;
// using UnityEditor;
// using UnityEngine;
//
// namespace Cardevil.Editors
// {
//     /// <summary>
//     /// <see cref="TileVector"/> attribute drawer.
//     /// </summary>
//     [CustomPropertyDrawer(typeof(TileVector))]
//     public class TileVectorDrawer : PropertyDrawer
//     {
//         
//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             // 한줄
//             return EditorGUIUtility.singleLineHeight * 2 +
//                    EditorGUIUtility.standardVerticalSpacing; 
//         }
//         private readonly GUIContent _iContent = new GUIContent("i");
//         private readonly GUIContent _jContent = new GUIContent("j");
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, property);
//             
//             var iProp = property.FindPropertyRelative("_i");
//             var jProp = property.FindPropertyRelative("_j");
//             var labelRect  = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
//     
//             EditorGUI.LabelField(labelRect, property.displayName);
//     
//             
//             var valueRect = new Rect(position.x + EditorGUIUtility.labelWidth,
//                 position.y + EditorGUIUtility.singleLineHeight +
//                 EditorGUIUtility.standardVerticalSpacing,
//                 position.width-EditorGUIUtility.labelWidth - 4, EditorGUIUtility.singleLineHeight);
//
//             float labelWidth = 5f;
//             float w = valueRect.width / 2f;
//             var iRect = new Rect(valueRect.x, valueRect.y, w, valueRect.height);
//             var jRect = new Rect(valueRect.x + w, valueRect.y, w, valueRect.height);
//             int newI = EditorGUI.IntField(iRect, _iContent, iProp.intValue);
//             int newJ = EditorGUI.IntField(jRect, _jContent, jProp.intValue);
//             if (newI != iProp.intValue || newJ != jProp.intValue)
//             {
//                 iProp.intValue = newI;
//                 jProp.intValue = newJ;
//                 property.serializedObject.ApplyModifiedProperties();
//             }
//             
//             
//             EditorGUIUtility.labelWidth = labelWidth;
//             EditorGUI.EndProperty();
//         }
//     }
// }