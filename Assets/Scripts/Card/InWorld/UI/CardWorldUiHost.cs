using Cardevil.Card.InWorld.UI.Selection;
using Cardevil.Card.InWorld.UI.Upgrade;
using Cardevil.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.UI
{
    public class CardWorldUiHost : MonoBehaviour
    {
        private const string PrefabPath = "UI/CardFlow/CardWorldUiHost";
        private const float MainIconSize = 72f;
        private const float MainIconGap = 14f;

        [field: SerializeField] public CardSelectionView SelectionView { get; private set; }
        [field: SerializeField] public CardUpgradeView UpgradeView { get; private set; }

        [Header("Common UI")]
        [SerializeField] private Image mainIcon;
        [SerializeField] private TextMeshProUGUI mainText;

        private void Awake()
        {
            ApplyCanvasSorting();
            ApplyMainHeaderLayout();
        }

        public static CardWorldUiHost Instantiate()
        {
            var go = AssetUtil.Instantiate(PrefabPath);
            if (!go)
            {
                Debug.LogError($"Failed to instantiate card UI host prefab: Resources/Prefabs/{PrefabPath}");
                return null;
            }

            var host = go.GetComponent<CardWorldUiHost>();
            if (!host)
            {
                Debug.LogError($"Card UI host prefab does not contain {nameof(CardWorldUiHost)}: {PrefabPath}");
                Object.Destroy(go);
                return null;
            }

            return host;
        }

        public void SetMainUi(Sprite icon, string text)
        {
            SetMainIcon(icon);
            SetMainText(text);
        }

        public void SetMainIcon(Sprite icon)
        {
            if (!mainIcon)
            {
                return;
            }

            mainIcon.sprite = icon;
            mainIcon.enabled = icon != null;
            ApplyMainHeaderLayout();
        }

        public void SetMainText(string text)
        {
            if (!mainText)
            {
                return;
            }

            mainText.text = text ?? string.Empty;
            ApplyMainHeaderLayout();
        }

        private void ApplyCanvasSorting()
        {
            int popupLayer = SortingLayer.NameToID(CardWorldUiSorting.PopupSortingLayerName);
            foreach (var canvas in GetComponentsInChildren<Canvas>(true))
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerID = popupLayer;
                canvas.sortingOrder = ResolveCanvasSortingOrder(canvas);
            }
        }

        private static int ResolveCanvasSortingOrder(Canvas canvas)
        {
            if (IsDimCanvas(canvas))
            {
                return (int)CardWorldUiSorting.Order.Dim;
            }

            if (IsCommonUiCanvas(canvas))
            {
                return (int)CardWorldUiSorting.Order.CommonUi;
            }

            return (int)CardWorldUiSorting.Order.Ui;
        }

        private static bool IsDimCanvas(Canvas canvas)
        {
            return canvas && canvas.name.Contains("Dim");
        }

        private static bool IsCommonUiCanvas(Canvas canvas)
        {
            return canvas && canvas.transform.parent && canvas.transform.parent.name.Contains("Card Common UI");
        }

        private void ApplyMainHeaderLayout()
        {
            if (!mainText)
            {
                return;
            }

            var textRect = mainText.rectTransform;
            bool hasIcon = mainIcon && mainIcon.enabled && mainIcon.sprite;
            textRect.anchoredPosition = new Vector2(hasIcon ? (MainIconSize + MainIconGap) * 0.5f : 0f, textRect.anchoredPosition.y);

            if (!mainIcon)
            {
                return;
            }

            var iconRect = mainIcon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(1f, 0.5f);
            iconRect.sizeDelta = new Vector2(MainIconSize, MainIconSize);
            iconRect.anchoredPosition = new Vector2(-MainIconGap, 0f);
            mainIcon.preserveAspect = true;
        }
    }
}
