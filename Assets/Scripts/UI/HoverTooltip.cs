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
        [field: TextArea]
        public string Description { get; set; }
        
        
    }
    
    
    public class HoverTooltip : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _titleText;
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
            if (string.IsNullOrEmpty(data.Title))
            {
                _titleText.gameObject.SetActive(false);
            }
            else
            {
                _titleText.gameObject.SetActive(true);
            }
            _titleText.text = data.Title;
            if (string.IsNullOrEmpty(data.Description))
            {
                _descriptionText.gameObject.SetActive(false);
            }
            else
            {
                _descriptionText.gameObject.SetActive(true);
            }

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
                
                // 0(좌하단), 1(좌상단), 2(우상단), 3(우하단)
                Vector3[] targetCorners = new Vector3[4];
                target.GetWorldCorners(targetCorners);
                
                Vector3 bottomCenterWorld = (targetCorners[0] + targetCorners[3]) / 2f;
                
                CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();
                Vector3[] canvasCorners = new Vector3[4];
                canvasScaler.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);

                float tooltipWorldWidth = _rectTransform.rect.width * _rectTransform.lossyScale.x;
                
                float pivotX = 0.5f;

                // 우측을 벗어나는 경우
                if (bottomCenterWorld.x + (tooltipWorldWidth / 2f) > canvasCorners[3].x)
                {
                    pivotX = 1f;
                }
                // 좌측을 벗어나는 경우
                else if (bottomCenterWorld.x - (tooltipWorldWidth / 2f) < canvasCorners[0].x)
                {
                    pivotX = 0f;
                }
                
                _rectTransform.pivot = new Vector2(pivotX, 1f);
                
                _rectTransform.position = bottomCenterWorld;
            }
        }
    }
}