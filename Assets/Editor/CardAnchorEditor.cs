using UnityEngine;
using UnityEditor;
using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.InWorld.UI;

namespace Cardevil.Card.EditorTools
{
    [UnityEditor.CustomEditor(typeof(CardAnchor))]
    public class CardAnchorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CardAnchor anchor = (CardAnchor)target;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("cardPrefab"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showPreview"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cardScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyUnityLayer"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unityLayerName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applySorting"));

            var sortingLayerProp = serializedObject.FindProperty("sortingLayerID");
            var orderInLayerProp = serializedObject.FindProperty("orderInLayer");
            var sortingLayers = SortingLayer.layers;
            var sortingLayerNames = new string[sortingLayers.Length];
            var sortingLayerIds = new int[sortingLayers.Length];
            for (int i = 0; i < sortingLayers.Length; i++)
            {
                sortingLayerNames[i] = sortingLayers[i].name;
                sortingLayerIds[i] = sortingLayers[i].id;
            }

            int newSortingLayerId = EditorGUILayout.IntPopup(
                "Sorting Layer",
                sortingLayerProp.intValue,
                sortingLayerNames,
                sortingLayerIds);

            if (newSortingLayerId != sortingLayerProp.intValue)
            {
                sortingLayerProp.intValue = newSortingLayerId;
            }

            EditorGUILayout.PropertyField(orderInLayerProp);

            if (anchor.mode == CardAnchor.PreviewMode.Custom)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Custom Card Settings", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("customType"));
                
                // Enforce valid UpgradePath for selected CardType
                if (anchor.customType == CardType.Attack && anchor.customUpgrade == UpgradePath.MultiDirection)
                {
                    serializedObject.FindProperty("customUpgrade").enumValueIndex = (int)UpgradePath.None;
                    serializedObject.ApplyModifiedProperties();
                }
                else if (anchor.customType == CardType.Move && 
                        (anchor.customUpgrade == UpgradePath.MultiNumber || anchor.customUpgrade == UpgradePath.MultiColor))
                {
                    serializedObject.FindProperty("customUpgrade").enumValueIndex = (int)UpgradePath.None;
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("customUpgrade"));

                if (anchor.customUpgrade != UpgradePath.None)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customElementCount"), new GUIContent("Upgrade Level (Count)"));
                }
                
                EditorGUILayout.Space();

                if (anchor.customType == CardType.Attack)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("baseColor"));
                    
                    if (anchor.customUpgrade == UpgradePath.MultiColor)
                    {
                        EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber1"), 2, 10, new GUIContent("Attack Number"));
                        if (anchor.customElementCount >= 1) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor1"));
                        if (anchor.customElementCount >= 2) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor2"));
                        if (anchor.customElementCount >= 3) EditorGUILayout.PropertyField(serializedObject.FindProperty("customColor3"));
                    }
                    else // None or MultiNumber
                    {
                        if (anchor.customElementCount >= 1 || anchor.customUpgrade == UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber1"), 2, 10);
                        if (anchor.customElementCount >= 2 && anchor.customUpgrade != UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber2"), 2, 10);
                        if (anchor.customElementCount >= 3 && anchor.customUpgrade != UpgradePath.None) 
                            EditorGUILayout.IntSlider(serializedObject.FindProperty("customNumber3"), 2, 10);
                    }
                }
                else // Move
                {
                    if (anchor.customUpgrade == UpgradePath.MultiDirection)
                    {
                        if (anchor.customElementCount >= 1) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir1"));
                        if (anchor.customElementCount >= 2) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir2"));
                        if (anchor.customElementCount >= 3) EditorGUILayout.PropertyField(serializedObject.FindProperty("customDir3"));
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
                
                if (!anchor.showPreview)
                {
                    anchor.ClearPreview();
                }
                else if (anchor.mode == CardAnchor.PreviewMode.Custom)
                {
                    anchor.GeneratePreview(false);
                }
                else if (anchor.mode == CardAnchor.PreviewMode.Random)
                {
                    anchor.GeneratePreview(false);
                }
            }

            EditorGUILayout.Space();
            
            if (GUILayout.Button("Randomize & Preview", GUILayout.Height(30)))
            {
                anchor.showPreview = true;
                anchor.GeneratePreview(true);
            }
        }
    }
}
