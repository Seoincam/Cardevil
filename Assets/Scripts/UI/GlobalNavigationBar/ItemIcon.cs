using Cardevil.UI;
using Cardevil.UI.Components;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavigationBar
{
    public class ItemIcon : UIBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Button useButton;
        [SerializeField] private ShowTooltipOnHover tooltip;

        private Action _useAction;
        private bool _isUsing;

        public bool HasItem { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ResolveReferences();
            Clear();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (useButton)
            {
                useButton.onClick.AddListener(Use);
            }
        }

        protected override void OnDisable()
        {
            if (useButton)
            {
                useButton.onClick.RemoveListener(Use);
            }

            base.OnDisable();
        }

        public void SetItem(Sprite icon, TooltipData tooltipData, Action useAction)
        {
            ResolveReferences();

            HasItem = true;
            _isUsing = false;
            _useAction = useAction;

            if (iconImage)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon;
                iconImage.gameObject.SetActive(icon);
            }

            if (tooltip)
            {
                tooltip.enabled = true;
                tooltip.SetTooltipData(tooltipData);
            }
        }

        public void Clear()
        {
            HasItem = false;
            _isUsing = false;
            _useAction = null;

            if (iconImage)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
                iconImage.gameObject.SetActive(false);
            }

            if (tooltip)
            {
                tooltip.HideTooltip();
                tooltip.SetTooltipData(null);
                tooltip.enabled = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Use();
        }

        private void Use()
        {
            if (!HasItem || _isUsing)
            {
                return;
            }

            _isUsing = true;
            var useAction = _useAction;
            Clear();
            useAction?.Invoke();
        }

        private void ResolveReferences()
        {
            if (!iconImage)
            {
                iconImage = transform.Find("ItemImage")?.GetComponent<Image>();
            }

            if (!iconImage)
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var image in images)
                {
                    if (image.gameObject != gameObject)
                    {
                        iconImage = image;
                        break;
                    }
                }
            }

            if (!useButton)
            {
                useButton = GetComponent<Button>();
            }

            if (!tooltip)
            {
                tooltip = GetComponent<ShowTooltipOnHover>();
            }

            if (!tooltip)
            {
                tooltip = gameObject.AddComponent<ShowTooltipOnHover>();
            }
        }
    }
}
