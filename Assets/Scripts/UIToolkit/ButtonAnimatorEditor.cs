#if UNITY_EDITOR
using Cardevil.UI;
using Cardevil.UI.Components;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.UIToolkit
{
    [CustomEditor(typeof(ButtonAnimator))]
    public class ButtonAnimatorEditor : Editor
    {
        private const string Path = "Assets/UI Toolkit/ButtonAnimator/ButtonAnimatorEditor.uxml";
        public VisualTreeAsset uxml;

        private void OnEnable()
        {
            if (!uxml)
                uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            if (!uxml)
                root.Add(new Label("ButtonAnimatorEditor.uxml을 찾지 못했습니다."));
            else
                uxml.CloneTree(root);    
            
            return root;
        }
    }
}
#endif