using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.UI.Upgrade
{
    public class CardUpgradeTooltipPanel : MonoBehaviour
    {
        private const float DefaultWidth = 400f;
        private const float DefaultMinHeight = 190f;

        [Header("Visual")]
        [SerializeField] private Image outlineImage;
        [SerializeField] private Image backgroundImage;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private GameObject selectedBadge;

        public RectTransform RectTransform => (RectTransform)transform;

        private void Awake()
        {
            ApplyReferenceLayout();
            SetSelected(false);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                ApplyReferenceLayout();
            }
        }

        public void Setup(string title, string description, int cost)
        {
            if (titleText)
            {
                titleText.text = title;
            }

            if (descriptionText)
            {
                descriptionText.text = description;
            }

            if (costText)
            {
                costText.text = $"{cost}G";
            }

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (outlineImage)
            {
                outlineImage.enabled = isSelected;
            }

            if (selectedBadge)
            {
                selectedBadge.SetActive(isSelected);
            }
        }

        private void ApplyReferenceLayout()
        {
            var rect = RectTransform;
            rect.pivot = new Vector2(0f, 0.5f);
            if (rect.sizeDelta.x <= 0f || rect.sizeDelta.y <= 0f)
            {
                rect.sizeDelta = new Vector2(DefaultWidth, DefaultMinHeight);
            }

            StretchToParent(backgroundImage ? backgroundImage.rectTransform : null, Vector2.zero);
            StretchToParent(outlineImage ? outlineImage.rectTransform : null, new Vector2(-4f, -4f));

            if (titleText)
            {
                titleText.textWrappingMode = TextWrappingModes.NoWrap;
                titleText.overflowMode = TextOverflowModes.Ellipsis;
            }

            if (descriptionText)
            {
                descriptionText.textWrappingMode = TextWrappingModes.Normal;
                descriptionText.overflowMode = TextOverflowModes.Truncate;
            }
        }

        private static void StretchToParent(RectTransform rect, Vector2 padding)
        {
            if (!rect)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = padding;
        }
    }
}
