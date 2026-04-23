using Cardevil.Card.Common.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Cardevil.Card.InWorld
{
    public class HandRankDescriptionView : MonoBehaviour
    {
        [SerializeField] private List<HandRankDescriptionRow> rows;
        [SerializeField] private TextMeshProUGUI handRankNameText;
        
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button exitButton;
        
        [Header("Settings")]
        [SerializeField] private List<HandRank> handRankOrders;
        
        [Header("Row UI")]
        [SerializeField] private Sprite selectedRowSprite;
        [SerializeField] private Sprite defaultRowSprite;

        private readonly Dictionary<HandRank, HandRankDescriptionRow> _rowMap = new(10);

        private HandRankDescriptionRow _currentSelectedRow;

        private void Awake()
        {
            for (int i = 0; i < handRankOrders.Count; i++)
            {
                var handRank = handRankOrders[i];
                var row = rows[i];

                row.Button.onClick.AddListener(() => HandleRowClicked(handRank));
                row.HandRank = handRank;
                row.Damage = 10;
                
                _rowMap.Add(handRank, row);
            }
            
            HandleRowClicked(handRankOrders[0]);

            if (exitButton != null)
            {
                exitButton.onClick.AddListener(HideAnimated);
            }
        }

        private void HandleRowClicked(HandRank targetHandRank)
        {
            handRankNameText.text = targetHandRank.ToString();

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
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1f, 0.3f);
        }

        [ContextMenu("Hide (Animated)")]
        public void HideAnimated()
        {
            if (canvasGroup == null) return;
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
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        [ContextMenu("Hide (Instant)")]
        public void HideInstant()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}