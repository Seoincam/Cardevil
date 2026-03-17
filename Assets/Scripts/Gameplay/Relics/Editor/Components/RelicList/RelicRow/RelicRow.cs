using Cardevil.Core.Utils;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cardevil.Gameplay.Relics.Editor.Components
{
    [UxmlElement]
    public partial class RelicRow : VisualElement
    {
        /// <summary>
        /// 삭제 버튼이 클릭됐을 때 삭제할 유물의 Id를 인자로 이벤트를 발행.
        /// </summary>
        public Action<string> DeleteClicked;

        private string _currentRelicId;
        
        // Simple Info
        private readonly Image _iconImage;
        private readonly Label _rarityLabel;
        
        // Text Info
        private readonly Label _titleLabel;
        private readonly Label _descLabel;
        
        // Management
        private readonly VisualElement _managementContainer;
        private readonly Button _deleteBtn;
        
        public RelicRow()
        {
            const string uxmlPath = "Assets/Scripts/Gameplay/Relics/Editor/Components/RelicList/RelicRow/RelicRow.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            if (!visualTree)
            {
                LogEx.LogError($"UXML 패스를 찾을 수 없음. 경로: {uxmlPath}");
                return;
            }

            visualTree.CloneTree(this);

            _iconImage = this.Q<Image>("Icon");
            _rarityLabel = this.Q<Label>("Rarity");
            
            _titleLabel = this.Q<Label>("Title");
            _descLabel = this.Q<Label>("Description");
            
            _managementContainer = this.Q("ManagementContainer");
            _deleteBtn = this.Q<Button>("DeleteButton");

            if (_deleteBtn != null)
            {
                _deleteBtn.clicked += () => DeleteClicked?.Invoke(_currentRelicId);
            }
        }

        public void SetupData(Sprite icon, string id, RelicRarity rarity, string title, string description, bool isLocal)
        {
            _currentRelicId = id;

            if (icon)
            {
                _iconImage.sprite = icon;
                _iconImage.tintColor = Color.white;
            }
            else
            {
                _iconImage.image = EditorGUIUtility.IconContent("d_Sprite Icon").image;
                _iconImage.tintColor = new Color(1, 1, 1, 0.5f);
            }

            Color rarityColor = rarity switch
            {
                RelicRarity.Default => Color.softYellow,
                RelicRarity.MiddleBoss => Color.lightCoral,
                RelicRarity.FinalBoss => Color.lightBlue
            };
            var rarityString = rarity switch
            {
                RelicRarity.Default => "Default",
                RelicRarity.MiddleBoss => "MidBoss",
                RelicRarity.FinalBoss => "FinBoss"
            };
            _rarityLabel.text = $"<color=#{ColorUtility.ToHtmlStringRGB(rarityColor)}>{rarityString}</color>";
            
            _titleLabel.text = $"{title} {GetIdRichText(id)}";
            _descLabel.text = description;
            
            _managementContainer.style.display = isLocal ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private string GetIdRichText(string id)
        {
            return $"<size=13><color=#32CD32>{id}</color></size>";
        }
    }
}