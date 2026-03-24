using Cardevil.Core.Utils;
using System;
using Cardevil.UI.Components;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI
{
    [Serializable]
    public class TooltipData
    {
        [field: SerializeField]
        public string Title { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string Description { get; set; }
        
        [SerializeReference] private List<TooltipData> subTooltips = new List<TooltipData>();
        
        public List<TooltipData> SubTooltips => subTooltips;
    }

    public class HoverTooltip : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TMPSizeFitter _descriptionSizeFitter;
        [SerializeField] private LayoutGroup _subTooltipLayoutGroup;

        [Header("레이아웃 옵션")]
        [SerializeField, Range(1, 3), Tooltip("자동 크기 안정화를 위해 레이아웃 강제 재계산을 반복하는 횟수")]
        private int _layoutRebuildPasses = 1;

        private CanvasScaler _canvasScaler;
        private readonly Vector3[] _targetCorners = new Vector3[4];
        private readonly Vector3[] _canvasCorners = new Vector3[4];

        private void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasScaler = GetComponentInParent<CanvasScaler>();

            if (_descriptionText == null)
            {
                _descriptionText = GetComponentInChildren<TextMeshProUGUI>(true);
            }

            if (_descriptionSizeFitter == null)
            {
                _descriptionSizeFitter = GetComponentInChildren<TMPSizeFitter>(true);
            }
        }

        private void Awake()
        {
            CacheReferences();
        }

        public void ShowTooltip(TooltipData data, RectTransform target = null, bool createSubTooltips = true)
        {
            CacheReferences();
            gameObject.SetActive(true);

            string title = data?.Title ?? string.Empty;
            string description = data?.Description ?? string.Empty;

            bool hasTitle = !string.IsNullOrEmpty(title);
            bool hasDescription = !string.IsNullOrEmpty(description);

            _titleText.gameObject.SetActive(hasTitle);
            _descriptionText.gameObject.SetActive(hasDescription);

            _titleText.text = title;
            _descriptionText.text = description;

            ApplyLayoutNow();

            if (target == null)
            {
                _rectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            target.GetWorldCorners(_targetCorners);
            Vector3 bottomCenterWorld = (_targetCorners[0] + _targetCorners[3]) * 0.5f;

            if (_canvasScaler == null)
            {
                _canvasScaler = GetComponentInParent<CanvasScaler>();
            }

            RectTransform canvasRectTransform = _canvasScaler != null ? _canvasScaler.GetComponent<RectTransform>() : null;
            if (canvasRectTransform == null)
            {
                _rectTransform.position = bottomCenterWorld;
                return;
            }

            canvasRectTransform.GetWorldCorners(_canvasCorners);

            float tooltipWorldWidth = _rectTransform.rect.width * _rectTransform.lossyScale.x;
            float halfWidth = tooltipWorldWidth * 0.5f;
            float pivotX = 0.5f;

            if (bottomCenterWorld.x + halfWidth > _canvasCorners[3].x)
            {
                pivotX = 1f;
            }
            else if (bottomCenterWorld.x - halfWidth < _canvasCorners[0].x)
            {
                pivotX = 0f;
            }

            _rectTransform.pivot = new Vector2(pivotX, 1f);
            _rectTransform.position = bottomCenterWorld;
            
            if (createSubTooltips)
            {
                CreateSubTooltips(data);
                ApplyLayoutNow();
            }
        }
        
        private void CreateSubTooltips(TooltipData data)
        {
            foreach (var subTooltipData in data.SubTooltips)
            {
                var subTooltipObj = AssetUtil.Instantiate("UI/HoverTooltip", _contentRoot);
                var subTooltip = subTooltipObj.GetComponent<HoverTooltip>();
                if (subTooltip != null)
                {
                    subTooltip.ShowTooltip(subTooltipData, null, createSubTooltips: false);
                    subTooltip.transform.SetParent(_subTooltipLayoutGroup.transform, false);
                }
            }
            
            _subTooltipLayoutGroup.gameObject.SetActive(data.SubTooltips.Count > 0);
        }

        private void ApplyLayoutNow()
        {
            _descriptionText.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            _descriptionSizeFitter?.Refresh(true);

            int passCount = Mathf.Max(1, _layoutRebuildPasses);
            for (int i = 0; i < passCount; i++)
            {
                if (_contentRoot != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
                _descriptionSizeFitter?.Refresh(false);
            }
        }

        private void CacheReferences()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_contentRoot == null && transform.childCount > 0)
            {
                _contentRoot = transform.GetChild(0) as RectTransform;
            }

            if (_canvasScaler == null)
            {
                _canvasScaler = GetComponentInParent<CanvasScaler>();
            }
        }
    }
}