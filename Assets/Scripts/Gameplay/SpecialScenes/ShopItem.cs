using Cardevil.Card.Common;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.InWorld.UI;
using Cardevil.UI;
using Cardevil.UI.Components;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public enum ShopEntryKind
    {
        Consumable,
        CardReinforce
    }

    public enum ShopConsumableKind
    {
        None,
        RedCard,
        GreenCard,
        BlueCard,
        BlackCard,
        RelicChest
    }

    [Serializable]
    public struct ShopDiscount
    {
        [Range(0f, 1f)] public float Percent;
        [Min(0)] public int FixedGold;

        public bool HasDiscount => Percent > 0f || FixedGold > 0;

        public int Apply(int baseCost)
        {
            int percentAdjusted = Mathf.CeilToInt(baseCost * (1f - Mathf.Clamp01(Percent)));
            return Mathf.Max(0, percentAdjusted - Mathf.Max(0, FixedGold));
        }
    }

    [Serializable]
    public struct ShopEntryData
    {
        public ShopEntryKind Kind;
        public int BaseGoldCost;
        public ShopDiscount Discount;
        public string TooltipKey;
        public ShopConsumableKind ConsumableKind;
        public string IconResourcePath;
        public int CardSpecId;
        public CardVisualInput CardVisualInput;
        public bool IsAvailable;

        public int FinalGoldCost => Discount.Apply(BaseGoldCost);
        public bool HasDiscount => Discount.HasDiscount && FinalGoldCost < BaseGoldCost;

        public static ShopEntryData Consumable(int baseGoldCost, string tooltipKey, ShopDiscount discount = default)
        {
            return Consumable(ShopConsumableKind.None, baseGoldCost, tooltipKey, null, discount);
        }

        public static ShopEntryData Consumable(
            ShopConsumableKind consumableKind,
            int baseGoldCost,
            string tooltipKey,
            string iconResourcePath,
            ShopDiscount discount = default)
        {
            return new ShopEntryData
            {
                Kind = ShopEntryKind.Consumable,
                BaseGoldCost = baseGoldCost,
                Discount = discount,
                TooltipKey = tooltipKey,
                ConsumableKind = consumableKind,
                IconResourcePath = iconResourcePath,
                CardSpecId = -1,
                IsAvailable = true
            };
        }

        public static ShopEntryData CardReinforce(
            int cardSpecId,
            CardVisualInput visualInput,
            int baseGoldCost,
            string tooltipKey,
            ShopDiscount discount = default)
        {
            return new ShopEntryData
            {
                Kind = ShopEntryKind.CardReinforce,
                BaseGoldCost = baseGoldCost,
                Discount = discount,
                TooltipKey = tooltipKey,
                CardSpecId = cardSpecId,
                CardVisualInput = visualInput,
                IsAvailable = true
            };
        }

        public Sprite LoadIcon()
        {
            return string.IsNullOrWhiteSpace(IconResourcePath)
                ? null
                : Resources.Load<Sprite>(IconResourcePath);
        }
    }

    [RequireComponent(typeof(ShowTooltipOnHover))]
    public class ShopItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ShowTooltipOnHover _tooltip;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Image _costIcon;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _originalCostText;
        [SerializeField] private Button _button;

        [Header("Card Entry References")]
        [SerializeField] private InteractionCard _cardPrefab;
        [SerializeField] private CardAnchor _cardAnchor;
        [SerializeField] private Transform _cardRoot;
        [SerializeField] private GameObject _cardVisualContainer;
        [SerializeField] private GameObject _itemIconContainer;

        public event Action<ShopEntryData> EntryClicked;
        public event Action OnItemClicked;

        private ShopEntryData _data;
        private InteractionCard _createdCard;
        private Image _entryIconImage;

        private void Awake()
        {
            _tooltip = GetComponent<ShowTooltipOnHover>();
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            ResolveCardAnchor();

            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleButtonClicked);
            }

            ClearCard();
        }

        public void Initialize(ShopEntryData data, TooltipData tooltipData, InteractionCard fallbackCardPrefab = null)
        {
            _data = data;
            gameObject.SetActive(data.IsAvailable);

            if (_button)
            {
                _button.interactable = data.IsAvailable;
            }

            ApplyCost(data);

            if (_tooltip)
            {
                _tooltip.SetTooltipData(tooltipData);
            }

            SetEntryVisual(data, fallbackCardPrefab);
        }

        public void SetInteractable(bool value)
        {
            if (_button)
            {
                _button.interactable = value && _data.IsAvailable;
            }
        }

        private void HandleButtonClicked()
        {
            if (!_data.IsAvailable)
            {
                return;
            }

            OnItemClicked?.Invoke();
            EntryClicked?.Invoke(_data);
        }

        private void SetEntryVisual(ShopEntryData data, InteractionCard fallbackCardPrefab)
        {
            bool isCardEntry = data.Kind == ShopEntryKind.CardReinforce;
            var cardAnchor = ResolveCardAnchor();
            var cardPrefab = _cardPrefab ? _cardPrefab : fallbackCardPrefab;
            bool useCardVisual = isCardEntry && cardAnchor && (cardAnchor.cardPrefab || cardPrefab);
            var entryIconImage = ResolveEntryIconImage();
            if (_cardVisualContainer)
            {
                _cardVisualContainer.SetActive(isCardEntry);
            }

            if (_itemIconContainer)
            {
                _itemIconContainer.SetActive(!isCardEntry);
            }
            else if (entryIconImage)
            {
                entryIconImage.gameObject.SetActive(!isCardEntry);
            }

            ClearCard();
            if (!isCardEntry && entryIconImage)
            {
                Sprite entryIcon = data.LoadIcon();
                entryIconImage.sprite = entryIcon;
                entryIconImage.enabled = entryIcon != null;
                entryIconImage.gameObject.SetActive(entryIcon != null);
            }

            if (!isCardEntry)
            {
                return;
            }

            if (!useCardVisual)
            {
                Debug.LogError($"{nameof(ShopItem)} card reinforce entries require a {nameof(CardAnchor)} with a card prefab.", this);
                return;
            }

            if (!cardAnchor.cardPrefab)
            {
                cardAnchor.cardPrefab = cardPrefab;
            }

            _createdCard = cardAnchor.Spawn(data.CardVisualInput);
        }

        private void ApplyCost(ShopEntryData data)
        {
            if (_costText)
            {
                _costText.text = _originalCostText
                    ? data.FinalGoldCost.ToString()
                    : FormatInlineCostText(data);
            }

            if (_originalCostText)
            {
                _originalCostText.gameObject.SetActive(data.HasDiscount);
                _originalCostText.text = data.HasDiscount ? $"<s>{data.BaseGoldCost}</s>" : string.Empty;
            }
        }

        private string FormatInlineCostText(ShopEntryData data)
        {
            return data.HasDiscount
                ? $"{data.FinalGoldCost} <s><color=#8E8E8E>{data.BaseGoldCost}</color></s>"
                : data.FinalGoldCost.ToString();
        }

        private CardAnchor ResolveCardAnchor()
        {
            if (_cardAnchor)
            {
                return _cardAnchor;
            }

            if (_cardRoot && _cardRoot.TryGetComponent(out CardAnchor anchorFromRoot))
            {
                _cardAnchor = anchorFromRoot;
                return _cardAnchor;
            }

            _cardAnchor = GetComponentInChildren<CardAnchor>(true);
            return _cardAnchor;
        }

        private Image ResolveEntryIconImage()
        {
            if (_entryIconImage)
            {
                return _entryIconImage;
            }

            if (_itemIconContainer)
            {
                foreach (var image in _itemIconContainer.GetComponentsInChildren<Image>(true))
                {
                    if (image && image != _costIcon)
                    {
                        _entryIconImage = image;
                        return _entryIconImage;
                    }
                }
            }

            if (_itemIcon && _itemIcon != _costIcon)
            {
                _entryIconImage = _itemIcon;
                return _entryIconImage;
            }

            _entryIconImage = _itemIcon;
            return _entryIconImage;
        }

        private void ClearCard()
        {
            if (!_createdCard)
            {
                return;
            }

            Destroy(_createdCard.gameObject);
            _createdCard = null;
        }
    }
}
