using Cardevil.Card.InWorld;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using Cardevil.Core.Utils;
using Cardevil.Gameplay;
using Cardevil.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.GlobalNavigationBar
{
    public class GlobalNavigationBar : MonoBehaviour
    {
        private static GlobalNavigationBar _instance;

        public static GlobalNavigationBar Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GlobalNavigationBar>(FindObjectsInactive.Include);
                }

                return _instance;
            }
        }

        [Header("References")]
        [Header("Player")]
        [SerializeField] private Image PlayerAvatar;
        [SerializeField] private TextMeshProUGUI PlayerLevel;
        [SerializeField] private TextMeshProUGUI PlayerName;
        [SerializeField] private TextMeshProUGUI HitPointAmount;
        [SerializeField] private TextMeshProUGUI GoldAmount;

        [SerializeField] private List<ConsumableIcon> consumableIcons = new();
        [SerializeField] private List<ItemIcon> itemIcons = new();
        [SerializeField, Min(1)] private int maxConsumableSlots = 5;

        [Space]
        [Header("Relics")]
        [SerializeField] private RelicBar relicBar;

        [Space]
        [Header("Menu")]
        [SerializeField] private Button deckButton;
        [SerializeField] private Button ranksButton;
        [SerializeField] private Button settingsButton;

        [Space(2f)]
        [Header("Settings")]
        [SerializeField] private ConsumableIcon consumableIconPrefab;
        // [SerializeField] private RelicIcon relicIconPrefab;
        [SerializeField] private RectTransform hidePositionTransform;

        [Space(2f)]
        [Header("Rank")]
        [SerializeField] private HandRankDescriptionView rankView;

        private RectTransform _rectTransform;
        private Vector2 _initialPosition;

        public RelicBar RelicBar => relicBar;

        public bool HasConsumableSlot
        {
            get
            {
                ResolveItemSlots();
                foreach (var slot in itemIcons)
                {
                    if (slot && !slot.HasItem)
                    {
                        return true;
                    }
                }

                return itemIcons.Count < Mathf.Max(1, maxConsumableSlots) && ResolveItemSlotTemplate();
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                LogEx.LogWarning("Multiple instances detected. Destroying duplicate GlobalNavigationBar.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform)
            {
                _initialPosition = _rectTransform.anchoredPosition;
            }

            ResolveItemSlots();

            if (ranksButton && rankView)
            {
                ranksButton.onClick.AddListener(() => rankView.ShowAnimated());
            }

            RefreshStatusTexts();
            Hide();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnEnable()
        {
            RefreshStatusTexts();
            ExecEventBus<PlayerStatusChangedArgs>.RegisterStatic(1000, OnPlayerStatusChanged);
        }

        private void OnDisable()
        {
            ExecEventBus<PlayerStatusChangedArgs>.UnregisterStatic(OnPlayerStatusChanged);
        }

        public void OnDeckButtonClicked()
        {
        }

        public void OnSettingsButtonClicked()
        {
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshStatusTexts();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool TryAddConsumable(Sprite icon, TooltipData tooltipData, Action useAction)
        {
            ResolveItemSlots();
            var targetSlot = FindEmptyItemSlot();
            if (!targetSlot)
            {
                targetSlot = CreateRuntimeItemSlot();
            }

            if (!targetSlot)
            {
                return false;
            }

            targetSlot.SetItem(icon, tooltipData, useAction);
            Show();
            return true;
        }

        public void RefreshStatusTexts()
        {
            if (CardevilCore.Instance == null ||
                CardevilCore.Instance.GameManager == null ||
                CardevilCore.Instance.GameManager.PlayerStatus == null)
            {
                return;
            }

            var status = CardevilCore.Instance.GameManager.PlayerStatus;

            if (PlayerLevel)
            {
                PlayerLevel.text = status.GetFinalValue(StatType.Level).ToString();
            }

            if (HitPointAmount)
            {
                HitPointAmount.text = $"{status.CurrentHp}/{status.MaxHp}";
            }

            if (GoldAmount)
            {
                GoldAmount.text = status.GetFinalValue(StatType.Gold).ToString();
            }
        }

        /// <summary>
        /// 위로 숨기기
        /// </summary>
        public UniTask HideAsync(float time = 0.3f, CancellationToken cancellationToken = default)
        {
            if (!_rectTransform || !hidePositionTransform)
            {
                Hide();
                return UniTask.CompletedTask;
            }

            _rectTransform.anchoredPosition = _initialPosition;
            return _rectTransform
                .DOAnchorPos(hidePositionTransform.anchoredPosition, time)
                .SetEase(Ease.InOutCubic)
                .ToUniTask(cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete);
        }

        /// <summary>
        /// 아래로 나타내기
        /// </summary>
        public UniTask ShowAsync(float time = 0.3f, CancellationToken cancellationToken = default)
        {
            Show();
            if (!_rectTransform || !hidePositionTransform)
            {
                return UniTask.CompletedTask;
            }

            _rectTransform.anchoredPosition = hidePositionTransform.anchoredPosition;
            return _rectTransform
                .DOAnchorPos(_initialPosition, time)
                .SetEase(Ease.InOutCubic)
                .ToUniTask(cancellationToken: cancellationToken, tweenCancelBehaviour: TweenCancelBehaviour.Complete);
        }

        [ContextMenu("Test Show")]
        public void TestShow()
        {
            ShowAsync().Forget();
        }

        [ContextMenu("Test Hide")]
        public void TestHide()
        {
            HideAsync().Forget();
        }

        private UniTask OnPlayerStatusChanged(PlayerStatusChangedArgs args, CancellationToken cancellationToken)
        {
            RefreshStatusTexts();
            return UniTask.CompletedTask;
        }

        private void ResolveItemSlots()
        {
            itemIcons ??= new List<ItemIcon>();
            itemIcons.RemoveAll(slot => !slot);
            if (itemIcons.Count > 0)
            {
                EnsureItemSlotParentCapacity();
                return;
            }

            itemIcons.AddRange(GetComponentsInChildren<ItemIcon>(true));
            EnsureItemSlotParentCapacity();
        }

        private ItemIcon FindEmptyItemSlot()
        {
            foreach (var slot in itemIcons)
            {
                if (slot && !slot.HasItem)
                {
                    return slot;
                }
            }

            return null;
        }

        private ItemIcon ResolveItemSlotTemplate()
        {
            ResolveItemSlots();
            foreach (var slot in itemIcons)
            {
                if (slot)
                {
                    return slot;
                }
            }

            return null;
        }

        private ItemIcon CreateRuntimeItemSlot()
        {
            if (itemIcons.Count >= Mathf.Max(1, maxConsumableSlots))
            {
                return null;
            }

            var template = ResolveItemSlotTemplate();
            if (!template || !template.transform.parent)
            {
                return null;
            }

            var slot = Instantiate(template, template.transform.parent);
            slot.name = $"{template.name}_{itemIcons.Count + 1}";
            slot.gameObject.SetActive(true);
            slot.Clear();
            itemIcons.Add(slot);
            EnsureItemSlotParentCapacity();
            return slot;
        }

        private void EnsureItemSlotParentCapacity()
        {
            var template = GetFirstItemSlot();
            if (!template)
            {
                return;
            }

            if (template.transform.parent is not RectTransform parentRect ||
                template.transform is not RectTransform slotRect)
            {
                return;
            }

            float slotWidth = slotRect.rect.width > 0f ? slotRect.rect.width : slotRect.sizeDelta.x;
            if (slotWidth <= 0f)
            {
                return;
            }

            int targetCount = Mathf.Max(itemIcons.Count, Mathf.Max(1, maxConsumableSlots));
            float spacing = 0f;
            float padding = 0f;
            if (parentRect.TryGetComponent(out HorizontalLayoutGroup layout))
            {
                spacing = layout.spacing;
                padding = layout.padding.left + layout.padding.right;
            }

            float targetWidth = padding + slotWidth * targetCount + spacing * Mathf.Max(0, targetCount - 1);
            if (parentRect.rect.width + 0.1f < targetWidth)
            {
                parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
            }
        }

        private ItemIcon GetFirstItemSlot()
        {
            foreach (var slot in itemIcons)
            {
                if (slot)
                {
                    return slot;
                }
            }

            return null;
        }
    }
}
