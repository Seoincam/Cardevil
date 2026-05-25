using UnityEngine;
using UnityEditor;
using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;

namespace Cardevil.Card.EditorTools
{
    [UnityEditor.CustomEditor(typeof(CardAnchorPreview))]
    public class CardAnchorPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CardAnchorPreview previewer = (CardAnchorPreview)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("cardPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPreview"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cardScale"));

            if (previewer.mode == CardAnchorPreview.PreviewMode.Custom)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Custom Card Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customType"));
                
                // Enforce valid UpgradePath for selected CardType
                if (previewer.customType == CardType.Attack && previewer.customUpgrade == UpgradePath.MultiDirection)
                {
                    serializedObject.FindProperty("customUpgrade").enumValueIndex = (int)UpgradePath.None;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (previewer.customType == CardType.Move && 
                        (previewer.customUpgrade == UpgradePath.MultiNumber || previewer.customUpgrade == UpgradePath.MultiColor))
                {
                    serializedObject.FindProperty("customUpgrade").enumValueIndex = (int)UpgradePath.None;
                    serializedObject.ApplyModifiedProperties();
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customUpgrade"));

                if (previewer.customUpgrade != UpgradePath.None)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customElementCount"), new GUIContent("Upgrade Level (Count)"));
                }
                
                EditorGUILayout.Space();

                if (previewer.customType == CardType.Attack)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("baseColor"));
                    
                    if (previewer.customUpgrade == UpgradePath.MultiColor)
                    {
                        EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber1"), 2, 10, new GUIContent("Attack Number"));
                        if (previewer.customElementCount >= 1) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor1"));
                        if (previewer.customElementCount >= 2) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor2"));
                        if (previewer.customElementCount >= 3) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor3"));
                    }
                    else // None or MultiNumber
                    {
                        if (previewer.customElementCount >= 1 || previewer.customUpgrade == UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber1"), 2, 10);
                        if (previewer.customElementCount >= 2 && previewer.customUpgrade != UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber2"), 2, 10);
                        if (previewer.customElementCount >= 3 && previewer.customUpgrade != UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber3"), 2, 10);
                    }
                }
                else // Move
                {
                    if (previewer.customUpgrade == UpgradePath.MultiDirection)
                    {
                        if (previewer.customElementCount >= 1) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir1"));
                        if (previewer.customElementCount >= 2) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir2"));
                        if (previewer.customElementCount >= 3) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir3"));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir1"), new GUIContent("Direction"));
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                
                if (!previewer.showPreview)
                {
                    previewer.ClearPreview();
                }
                else if (previewer.mode == CardAnchorPreview.PreviewMode.Custom)
                {
                    previewer.GeneratePreview(false);
                }
                else if (previewer.mode == CardAnchorPreview.PreviewMode.Random)
                {
                    previewer.GeneratePreview(false);
                }
            }

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Randomize & Preview", GUILayout.Height(30)))
            {
                previewer.showPreview = true;
                previewer.GeneratePreview(true);
            }
        }
    }
}
