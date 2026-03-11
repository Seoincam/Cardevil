using Cardevil.Core.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Cardevil.Editors
{
    /// <summary>
    /// <see cref="VisibleOnly"/> attribute drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(VisibleOnly))]
    public class VisibleOnlyDrawer : PropertyDrawer
    {
        // Queue<SerializedProperty> queue = new Queue<SerializedProperty>();
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool originalGuiEnabled = GUI.enabled;
            bool editable = false;
            VisibleOnly target = attribute as VisibleOnly;
            Debug.Assert(target != null, "VisibleOnly attribute is null");
            if (originalGuiEnabled == false && target.IgnoreParentEditable == false)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }
            if (Application.isPlaying)
            {
                if (target.EditableIn == EditableIn.PlayMode)
                {
                    editable = true;
                }
            }
            else
            {
                if (target.EditableIn == EditableIn.EditMode)
                {
                    editable = true;
                }
            }
            if (editable)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }
    
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = originalGuiEnabled;
        }
    }
}
