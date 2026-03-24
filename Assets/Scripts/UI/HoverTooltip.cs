using Cardevil.UI.PopUp;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI
{
    [Serializable]
    public class TooltipData
    {
        [field: SerializeField]
        public string Title{ get; set; }
        [field: SerializeField]
        public string Description { get; set; }
        
        
    }
    
    
    public class HoverTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private RectTransform _targetRectTransform;

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _descriptionText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Awake()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
        }

        public void ShowTooltip(TooltipData data, RectTransform target = null)
        {
            gameObject.SetActive(true);
            _descriptionText.text = data.Description;
            _targetRectTransform = target;
            var halfScreenWidth = GetComponentInParent<CanvasScaler>().referenceResolution.x / 2f;
            var tooltipWidth = _rectTransform.rect.width;
            if (target == null)
            {
                _rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                // 타겟 위치에 따라 툴팁이 화면 밖으로 나가지 않도록 피벗과 위치 조정
                // 타겟은 rect의 아래중앙
                var targetPos = target.rect.center;
                var pivotX = 0.5f;
                if (targetPos.x + tooltipWidth / 2f > halfScreenWidth)
                {
                    pivotX = 1f; // 오른쪽 끝에 붙도록
                }
                else if (targetPos.x - tooltipWidth / 2f < -halfScreenWidth)
                {
                    pivotX = 0f; // 왼쪽 끝에 붙도록
                }
                _rectTransform.pivot = new Vector2(pivotX, 1f);
                _rectTransform.anchoredPosition = targetPos;
            }
        }
    }
}