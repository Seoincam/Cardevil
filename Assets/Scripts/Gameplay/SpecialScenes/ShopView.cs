using Cardevil.Card.Common;
using Cardevil.Core.Systems;
using Cardevil.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cardevil.Gameplay.SpecialScenes
{
    public class ShopView : SpecialSceneView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Shop References")]
        [SerializeField] private Transform shopItemContainer;
        [SerializeField] private Transform reinforceItemContainer;

        [FormerlySerializedAs("shopItems")]
        [SerializeField] private List<ShopItem> consumableItems = new(5);
        [SerializeField] private List<ShopItem> reinforceItems = new(10);
        [SerializeField] private Button reinforceButton;
        [SerializeField] private Button exitButton;
        public event Action<ShopEntryData> EntryClicked;

        ShopCore shopCore;
        private InteractionCard _cardEntryPrefab;

        private void Awake()
        {
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            SetItemsActive(consumableItems, false);
            SetItemsActive(reinforceItems, false);
            HideLegacyReinforceButton();
        }

        public void SetCardEntryPrefab(InteractionCard cardEntryPrefab)
        {
            _cardEntryPrefab = cardEntryPrefab;
        }

        public override void Bind(SpecialSceneCore core)
        {
            shopCore = (ShopCore)core;
            if (shopCore == null)
            {
                Debug.LogError($"{nameof(ShopView)} requires {nameof(ShopCore)}.", this);
                return;
            }

            if (exitButton)
            {
                exitButton.onClick.RemoveListener(HandleExitClicked);
                exitButton.onClick.AddListener(HandleExitClicked);
            }

            HideLegacyReinforceButton();

            ValidatePreplacedItemCapacity(consumableItems, shopCore.ConsumableEntries.Count, nameof(consumableItems));
            ValidatePreplacedItemCapacity(reinforceItems, shopCore.ReinforceEntries.Count, nameof(reinforceItems));

            BindItems(consumableItems, shopCore.ConsumableEntries, null);
            BindItems(reinforceItems, shopCore.ReinforceEntries, _cardEntryPrefab);
        }

        private void OnDestroy()
        {
            if (exitButton)
            {
                exitButton.onClick.RemoveListener(HandleExitClicked);
            }

            UnbindItems(consumableItems);
            UnbindItems(reinforceItems);
        }

        private void HandleExitClicked()
        {
            RaiseCloseRequested();
        }

        public void SetEntriesInteractable(bool value)
        {
            SetItemsInteractable(consumableItems, value);
            SetItemsInteractable(reinforceItems, value);

            if (exitButton)
            {
                exitButton.interactable = value;
            }

            if (reinforceButton)
            {
                reinforceButton.interactable = false;
            }
        }

        private void HandleEntryClicked(ShopEntryData entry)
        {
            EntryClicked?.Invoke(entry);
        }

        private void ValidatePreplacedItemCapacity(IReadOnlyCollection<ShopItem> items, int requiredCount, string listName)
        {
            if (items != null && requiredCount <= items.Count)
            {
                return;
            }

            int actualCount = items?.Count ?? 0;
            Debug.LogError(
                $"{nameof(ShopView)} requires {requiredCount} preplaced {nameof(ShopItem)} slots in {listName}. Found: {actualCount}.",
                this);
        }

        private void HideLegacyReinforceButton()
        {
            if (!reinforceButton)
            {
                return;
            }

            reinforceButton.interactable = false;
            reinforceButton.gameObject.SetActive(false);
        }

        private void BindItems(
            IReadOnlyList<ShopItem> items,
            IReadOnlyList<ShopEntryData> entries,
            InteractionCard cardEntryPrefab)
        {
            if (items == null)
            {
                return;
            }

            entries ??= Array.Empty<ShopEntryData>();
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (!item)
                {
                    continue;
                }

                item.EntryClicked -= HandleEntryClicked;

                bool hasEntry = i < entries.Count && entries[i].IsAvailable;
                item.gameObject.SetActive(hasEntry);
                if (!hasEntry)
                {
                    continue;
                }

                TooltipData tooltipData = ResolveShopTooltip(entries[i]);
                item.Initialize(entries[i], tooltipData, cardEntryPrefab);
                item.EntryClicked += HandleEntryClicked;
            }
        }

        public static TooltipData ResolveShopTooltip(ShopEntryData entry)
        {
            TooltipData tooltipData = TooltipResolver.Resolve(entry.TooltipKey);
            if (entry.Kind == ShopEntryKind.Consumable)
            {
                return ResolveConsumableTooltip(entry.ConsumableKind, tooltipData);
            }

            if (tooltipData != null &&
                !string.IsNullOrWhiteSpace(tooltipData.Description) &&
                !tooltipData.Description.StartsWith("Missing Tooltip", StringComparison.Ordinal))
            {
                return tooltipData;
            }

            return entry.Kind switch
            {
                ShopEntryKind.CardReinforce => new TooltipData
                {
                    Title = "카드 강화",
                    Description = "골드를 지불해 선택한 카드를 다음 강화 단계로 확정 강화합니다."
                },
                _ => tooltipData
            };
        }

        private static TooltipData ResolveConsumableTooltip(ShopConsumableKind kind, TooltipData databaseTooltip)
        {
            TooltipData tooltipData = ResolveConsumableFallbackTooltip(kind);
            if (databaseTooltip != null &&
                !string.IsNullOrWhiteSpace(databaseTooltip.Title) &&
                !(databaseTooltip.Description ?? string.Empty).StartsWith("Missing Tooltip", StringComparison.Ordinal))
            {
                tooltipData.Title = databaseTooltip.Title;
            }

            return tooltipData;
        }

        private static TooltipData ResolveConsumableFallbackTooltip(ShopConsumableKind kind)
        {
            return kind switch
            {
                ShopConsumableKind.RedCard => new TooltipData
                {
                    Title = "레드 카드",
                    Description = "사용 시 공격 1회의 최종 데미지가 x1.5 증가합니다. GNB 아이템 슬롯에 보관됩니다."
                },
                ShopConsumableKind.GreenCard => new TooltipData
                {
                    Title = "그린 카드",
                    Description = "사용하면 체력을 1 회복하는 소비품입니다. GNB 아이템 슬롯에 보관됩니다."
                },
                ShopConsumableKind.BlueCard => new TooltipData
                {
                    Title = "블루 카드",
                    Description = "사용 시 리롤 티켓을 1개 얻습니다. GNB 아이템 슬롯에 보관됩니다."
                },
                ShopConsumableKind.BlackCard => new TooltipData
                {
                    Title = "블랙 카드",
                    Description = "사용 시 주사위를 굴려 골드, 회복, 피해 증가, 리롤 티켓, 보호막, 피해 중 하나가 발동합니다. GNB 아이템 슬롯에 보관됩니다."
                },
                ShopConsumableKind.RelicChest => new TooltipData
                {
                    Title = "유물 상자",
                    Description = "사용 시 아직 보유하지 않은 기본 유물 3개 중 하나를 선택해 얻습니다. GNB 아이템 슬롯에 보관됩니다."
                },
                _ => new TooltipData
                {
                    Title = "소비품",
                    Description = "구매 후 GNB 아이템 슬롯에 보관되는 소비품입니다."
                }
            };
        }

        private void UnbindItems(IReadOnlyList<ShopItem> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item)
                {
                    item.EntryClicked -= HandleEntryClicked;
                }
            }
        }

        private static void SetItemsInteractable(IReadOnlyList<ShopItem> items, bool value)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item && item.gameObject.activeSelf)
                {
                    item.SetInteractable(value);
                }
            }
        }

        private static void SetItemsActive(IReadOnlyList<ShopItem> items, bool value)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item)
                {
                    item.gameObject.SetActive(value);
                }
            }
        }

        public override async UniTask PlayEnterAsync()
        {
            if (!canvasGroup)
            {
                return;
            }

            var overlayCanvas = UI.OverlayCanvas.Instance;
            var blackPanelCanvasGroup = overlayCanvas && overlayCanvas.BlackPanel
                ? overlayCanvas.BlackPanel.CanvasGroup
                : null;

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            var viewFade = canvasGroup.DOFade(1f, 0.2f).ToUniTask(TweenCancelBehaviour.Complete);
            if (!blackPanelCanvasGroup)
            {
                await viewFade;
                return;
            }

            var blackFade = blackPanelCanvasGroup.DOFade(0, 0.2f)
                .ToUniTask(TweenCancelBehaviour.Complete);
            await UniTask.WhenAll(blackFade, viewFade);
        }

        public override async UniTask PlayExitAsync()
        {
            if (!canvasGroup)
            {
                return;
            }

            await canvasGroup.DOFade(0f, 0.2f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // #region Legacy
        //
        //
        // [Header("Legacy References")]
  
        // [SerializeField] private Image panelImage;
        // [SerializeField] private TextMeshProUGUI titleText;
        // [SerializeField] private TextMeshProUGUI bodyText;
        // [SerializeField] private Button closeButton;
        // [SerializeField] private TextMeshProUGUI buttonText;
        //
        // private void Awake()
        // {
        //     if (closeButton)
        //     {
        //         closeButton.onClick.AddListener(RaiseCloseRequested);
        //     }
        // }
        //
        // public override void Bind(SpecialSceneCore core)
        // {
        //     titleText.text = core.TestTitle;
        //     bodyText.text = core.TestDescription;
        //     buttonText.text = core.TestConfirmLabel;
        //     panelImage.color = core.TestAccentColor;
        // }
        //
        // public override async UniTask PlayEnterAsync()
        // {
        //     var blackFade = UI.OverlayCanvas.Instance.BlackPanel.CanvasGroup.DOFade(0, 0.8f)
        //         .ToUniTask(TweenCancelBehaviour.Complete);
        //     canvasGroup.interactable = true;
        //     canvasGroup.blocksRaycasts = true;
        //     await blackFade;
        // }
        // #endregion
    }
}
