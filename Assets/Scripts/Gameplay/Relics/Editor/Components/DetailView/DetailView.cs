using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class DetailView : VisualElement
    {
        public event Action DataChanged;
        public event Action CloseClicked;
        public event Action<string> DeleteClicked;
        
        private readonly Button _closeButton;
        private readonly Button _deleteButton;
        private readonly RelicInformationBox _relicInformationBox;
        private readonly EffectList _effectList;

        private RelicSO _currentRelic;
        private SerializedObject _serializedRelic; // 추가: SerializedObject 캐싱 및 관리
        
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
            _deleteButton = this.Q<Button>("Delete");
            _relicInformationBox = this.Q<RelicInformationBox>();
            _effectList = this.Q<EffectList>();
            
            _relicInformationBox.DataChanged += () => DataChanged?.Invoke();
            _closeButton.clicked += () => CloseClicked?.Invoke();
            
            if (_deleteButton != null)
                _deleteButton.clicked += () => DeleteClicked?.Invoke(_currentRelic?.Data.Id);
        }
        
        public void BindRelic(RelicSO relic)
        {
            _currentRelic = relic;
            
            if (_deleteButton != null)
                _deleteButton.style.display = relic.FromSheet ? DisplayStyle.None : DisplayStyle.Flex;

            _serializedRelic?.Dispose();
            _serializedRelic = new SerializedObject(relic);
            
            _relicInformationBox.BindRelic(_serializedRelic);
            _effectList.BindRelic(_serializedRelic);
        }
    }
}