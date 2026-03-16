using Cardevil.Core.Utils;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class DetailView : VisualElement
    {
        public event Action CloseClicked;
        
        private readonly Button _closeButton;
        private readonly RelicInformationBox _relicInformationBox;
        private readonly EffectList _effectList;
        
        public DetailView()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/DetailView/DetailView.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }
            
            visualTree.CloneTree(this);

            _closeButton = this.Q<Button>("Close");
            _relicInformationBox = this.Q<RelicInformationBox>("RelicInformationBox");
            _effectList = this.Q<EffectList>("EffectList");
            
            if (_closeButton != null)
                _closeButton.clicked += () => CloseClicked?.Invoke();
        }

        public void BindRelic(SerializedObject serializedRelic)
        {
            _relicInformationBox.BindRelic(serializedRelic);
            _effectList.BindRelic(serializedRelic);
        }
    }
}