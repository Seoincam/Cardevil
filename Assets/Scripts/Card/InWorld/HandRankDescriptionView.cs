using Cardevil.Card.Common;
using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Visual;
using Cardevil.Core.Bootstrap;
using Database.Generated;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;

namespace Cardevil.Card.InWorld
{
    public class HandRankDescriptionView : MonoBehaviour
    {
        [SerializeField] private Camera cardCamera;
        
        [Header("UI")]
        [SerializeField] private List<HandRankDescriptionRow> rows;
        [SerializeField] private TextMeshProUGUI handRankNameText;
        [SerializeField] private TextMeshProUGUI handRankDescriptionText;
        
        [Header("Row UI")]
        [SerializeField] private Sprite selectedRowSprite;
        [SerializeField] private Sprite defaultRowSprite;
        
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button exitButton;
        
        [Header("Settings")]
        [SerializeField] private List<HandRank> handRankOrders;
        
        [Header("Cards")]
        [SerializeField] private List<InteractionCard> cards;

        private readonly Dictionary<HandRank, HandRankDescriptionRow> _rowMap = new(10);

        private HandRankDescriptionRow _currentSelectedRow;

        private void Awake()
        {
            for (int i = 0; i < handRankOrders.Count; i++)
            {
                var handRank = handRankOrders[i];
                var row = rows[i];
                var handRankData = GetHandRankData(handRank);

                row.Button.onClick.AddListener(() => HandleRowClicked(handRank));
                row.HandRank = handRankData.DisplayName;
                row.Damage = handRankData.Value;
                
                _rowMap.Add(handRank, row);
            }
            
            HandleRowClicked(handRankOrders[0]);

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(HideAnimated);
            }

            foreach (var card in cards)
            {
                var visualInput = CardVisualInput.Attack(CardColor.Black, 3);
                card.Initialize(visualInput);
            }
            
            HideInstant();
        }

        private void HandleRowClicked(HandRank targetHandRank)
        { 
            var data = GetHandRankData(targetHandRank);
            handRankNameText.text = data.DisplayName;
            handRankDescriptionText.text = data.DisplayCondition;

            if (data.DisplayCardColors is { Count: 4 } && data.DisplayCardNumbers is { Count: 4 })
            {
                for (int i = 0; i < 4; i++)
                {
                    var visualInput = CardVisualInput.Attack(data.DisplayCardColors[i], data.DisplayCardNumbers[i]);
                    cards[i].VisualController.SetLayout(visualInput);
                }
            }

            if (_currentSelectedRow)
            {
                _currentSelectedRow.Button.image.sprite = defaultRowSprite;
            }

            _currentSelectedRow = _rowMap[targetHandRank];
            _currentSelectedRow.Button.image.sprite = selectedRowSprite;
        }

        [ContextMenu("Show (Animated)")]
        public void ShowAnimated()
        {
            if (canvasGroup == null) return;
            
            HandleRowClicked(handRankOrders[0]);
            
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            
            foreach (var card in cards)
            {
                card.VisualController.DoFade(1f, 0.3f, Ease.Unset, true);
            }
            canvasGroup.DOFade(1f, 0.3f);
        }

        [ContextMenu("Hide (Animated)")]
        public void HideAnimated()
        {
            if (canvasGroup == null) return;

            foreach (var card in cards)
            {
                card.VisualController.DoFade(0f, 0.3f, Ease.Unset, true);
            }
            
            canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            });
        }

        [ContextMenu("Show (Instant)")]
        public void ShowInstant()
        {
            if (canvasGroup == null) return;
            
            HandleRowClicked(handRankOrders[0]);
            canvasGroup.alpha = 1f;
            foreach (var card in cards)
            {
                card.VisualController.Fade(1f, true);
            }
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        [ContextMenu("Hide (Instant)")]
        public void HideInstant()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            foreach (var card in cards)
            {
                card.VisualController.Fade(0f, true);
            }
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private static HandRankData GetHandRankData(HandRank handRank)
        {
            var data = CardevilCore.Database.Database.HandRankDataList
                .FirstOrDefault(d => d.Ranking == handRank);

            if (data == null) throw new ArgumentException($"{handRank} 데이터를 찾을 수 없음.");

            return data;
        }
    }
}