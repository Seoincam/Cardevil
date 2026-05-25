using Cardevil.Card.Common;
using Cardevil.Card.Common.Core.Upgrade;
using Cardevil.Card.Common.Visual;
using Cardevil.Card.EditorTools;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Card.InWorld.UI.Upgrade
{
    public class CardUpgradeView : MonoBehaviour
    {
        private const float SlotClickPaddingPx = 24f;
        private static readonly Vector2 FallbackCardClickSizePx = new(260f, 380f);
        private const float SelectedCardScaleMultiplier = 1.08f;
        private const float SelectionTweenDuration = 0.12f;

        public event Action<UpgradeNodeSO> SelectedNodeChanged;
        public event Action<UpgradeNodeSO> ConfirmClicked;
        public event Action CloseClicked;

        [Header("Prefabs")]
        [SerializeField] private InteractionCard cardPrefab;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 1f;

        [Header("Card Anchors")]
        [SerializeField] private CardAnchor originalAnchor;
        [SerializeField] private CardAnchor next_1_0Anchor;
        [SerializeField] private CardAnchor next_2_0Anchor;
        [SerializeField] private CardAnchor next_2_1Anchor;

        [Space, SerializeField] private CanvasGroup canvasGroup;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Tooltip Panels")]
        [SerializeField] private CardUpgradeTooltipPanel tooltipPanel_1_0;
        [SerializeField] private CardUpgradeTooltipPanel tooltipPanel_2_0;
        [SerializeField] private CardUpgradeTooltipPanel tooltipPanel_2_1;

        [Header("Arrows")]
        [SerializeField] private GameObject arrow_1_0;
        [SerializeField] private GameObject arrow_2_0;
        [SerializeField] private GameObject arrow_2_1;

        [Header("Canvas")]
        [SerializeField] private Canvas controlsCanvas;

        private readonly Dictionary<InteractionCard, UpgradeNodeSO> _cardMap = new(2);
        private readonly Dictionary<InteractionCard, Vector3> _cardBaseScales = new(2);
        private readonly List<CandidateSlot> _candidateSlots = new(2);
        private InteractionCard _originalCard;
        private InteractionCard _currentSelectedCard;

        private sealed class CandidateSlot
        {
            public InteractionCard Card;
            public UpgradeNodeSO Node;
            public CardUpgradeTooltipPanel TooltipPanel;
            public RectTransform ClickArea;
            public Button ClickButton;
            public UnityEngine.Events.UnityAction ClickAction;
        }

        private void Awake()
        {
            SetInteractable(false);

            if (cancelButton)
            {
                cancelButton.onClick.AddListener(HandleCloseClicked);
            }

            if (confirmButton)
            {
                confirmButton.onClick.AddListener(HandleConfirmClicked);
            }
        }

        private void OnDestroy()
        {
            if (cancelButton)
            {
                cancelButton.onClick.RemoveListener(HandleCloseClicked);
            }

            if (confirmButton)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }
        }

        public void Configure(
            InteractionCard cardPrefab,
            CanvasGroup canvasGroup,
            Button confirmButton,
            Button cancelButton,
            TextMeshProUGUI costText,
            Transform originalAnchor,
            Transform next_1_0Anchor,
            Transform next_2_0Anchor,
            Transform next_2_1Anchor)
        {
            Configure(
                cardPrefab,
                canvasGroup,
                confirmButton,
                cancelButton,
                costText,
                ResolveAnchor(originalAnchor),
                ResolveAnchor(next_1_0Anchor),
                ResolveAnchor(next_2_0Anchor),
                ResolveAnchor(next_2_1Anchor));
        }

        public void Configure(
            InteractionCard cardPrefab,
            CanvasGroup canvasGroup,
            Button confirmButton,
            Button cancelButton,
            TextMeshProUGUI costText,
            CardAnchor originalAnchor,
            CardAnchor next_1_0Anchor,
            CardAnchor next_2_0Anchor,
            CardAnchor next_2_1Anchor)
        {
            this.cardPrefab = cardPrefab;
            this.canvasGroup = canvasGroup;
            this.confirmButton = confirmButton;
            this.cancelButton = cancelButton;
            this.costText = costText;
            this.originalAnchor = originalAnchor;
            this.next_1_0Anchor = next_1_0Anchor;
            this.next_2_0Anchor = next_2_0Anchor;
            this.next_2_1Anchor = next_2_1Anchor;

            if (this.cancelButton)
            {
                this.cancelButton.onClick.RemoveListener(HandleCloseClicked);
                this.cancelButton.onClick.AddListener(HandleCloseClicked);
            }

            if (this.confirmButton)
            {
                this.confirmButton.onClick.RemoveListener(HandleConfirmClicked);
                this.confirmButton.onClick.AddListener(HandleConfirmClicked);
            }

            SetInteractable(false);
        }

        public void Create(CardVisualInput originalVisualInput,
            CardUpgradePresenter.UpgradeData candidateData)
        {
            ClearCards();
            HideAllTooltips();
            HideAllArrows();

            CreateOriginalCard(originalVisualInput, originalAnchor);

            var candidate = CreateCandidateSlot(candidateData, next_1_0Anchor, tooltipPanel_1_0);
            SetArrowVisible(arrow_1_0, true);

            HandleCardSelected(candidate?.Card);
        }

        public void Create(CardVisualInput originalVisualInput,
            CardUpgradePresenter.UpgradeData candidate1Data,
            CardUpgradePresenter.UpgradeData candidate2Data)
        {
            ClearCards();
            HideAllTooltips();
            HideAllArrows();

            CreateOriginalCard(originalVisualInput, originalAnchor);

            var candidate1 = CreateCandidateSlot(candidate1Data, next_2_0Anchor, tooltipPanel_2_0);
            var candidate2 = CreateCandidateSlot(candidate2Data, next_2_1Anchor, tooltipPanel_2_1);

            SetArrowVisible(arrow_2_0, true);
            SetArrowVisible(arrow_2_1, true);

            HandleCardSelected(candidate1?.Card);
        }

        public void ValidCanUpgrade(bool canUpgrade)
        {
            if (confirmButton)
            {
                confirmButton.interactable = canUpgrade;
            }
        }

        public async UniTask PlayOpenAnimationAsync()
        {
            await FadeIn();
        }

        public async UniTask PlayCloseAnimationAsync()
        {
            await FadeOut();
        }

        private void AttachTooltip(InteractionCard card, UpgradeNodeSO node,
            CardUpgradeTooltipPanel panel)
        {
            if (!card || !panel)
            {
                return;
            }

            panel.gameObject.SetActive(true);

            var (title, desc) = UpgradeTextResolver.GetTooltipText(node.Path, node.Stage);
            panel.Setup(title, desc, node.MarketCost);
        }

        private void HideAllTooltips()
        {
            if (tooltipPanel_1_0)
            {
                tooltipPanel_1_0.gameObject.SetActive(false);
            }

            if (tooltipPanel_2_0)
            {
                tooltipPanel_2_0.gameObject.SetActive(false);
            }

            if (tooltipPanel_2_1)
            {
                tooltipPanel_2_1.gameObject.SetActive(false);
            }
        }

        private async UniTask FadeIn()
        {
            foreach (var card in _cardMap.Keys)
            {
                card.VisualController.Fade(0f, true);
                _ = card.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }

            if (_originalCard)
            {
                _originalCard.VisualController.Fade(0f, true);
                _ = _originalCard.VisualController.DoFade(1f, fadeDuration, Ease.Unset, true);
            }

            canvasGroup.alpha = 0f;
            await canvasGroup.DOFade(1f, fadeDuration);

            SetInteractable(true);
        }

        private async UniTask FadeOut()
        {
            foreach (var card in _cardMap.Keys)
            {
                card.VisualController.Fade(1f, true);
                _ = card.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }

            if (_originalCard)
            {
                _originalCard.VisualController.Fade(1f, true);
                _ = _originalCard.VisualController.DoFade(0f, fadeDuration, Ease.Unset, true);
            }

            canvasGroup.alpha = 1f;
            await canvasGroup.DOFade(0f, fadeDuration);

            SetInteractable(false);

            ClearCards();
            HideAllTooltips();
            HideAllArrows();
        }

        private void HandleCardSelected(InteractionCard selected)
        {
            if (!selected)
            {
                return;
            }

            if (_currentSelectedCard == selected)
            {
                return;
            }

            _currentSelectedCard = selected;

            var node = _cardMap[selected];
            if (costText)
            {
                costText.text = $"{node.MarketCost}G";
            }

            foreach (var slot in _candidateSlots)
            {
                bool isSelected = slot.Card == selected;
                if (slot.TooltipPanel)
                {
                    slot.TooltipPanel.SetSelected(isSelected);
                }

                SetCardSelectedVisual(slot.Card, isSelected);
            }

            SelectedNodeChanged?.Invoke(node);
        }

        private void HandleConfirmClicked()
        {
            if (!_currentSelectedCard)
            {
                return;
            }

            ConfirmClicked?.Invoke(_cardMap[_currentSelectedCard]);
        }

        private void HandleCloseClicked()
        {
            CloseClicked?.Invoke();
        }

        private InteractionCard CreateOriginalCard(CardVisualInput originalVisualInput, CardAnchor anchor)
        {
            var card = SpawnCard(anchor, originalVisualInput);
            _originalCard = card;
            return card;
        }

        private CandidateSlot CreateCandidateSlot(CardUpgradePresenter.UpgradeData data,
            CardAnchor anchor, CardUpgradeTooltipPanel tooltipPanel)
        {
            var card = CreateCandidateCard(data, anchor);
            if (!card)
            {
                return null;
            }

            AttachTooltip(card, data.UpgradeNode, tooltipPanel);

            var slot = new CandidateSlot
            {
                Card = card,
                Node = data.UpgradeNode,
                TooltipPanel = tooltipPanel
            };

            card.PointerUp += HandleCardSelected;
            CreateClickArea(slot);
            _candidateSlots.Add(slot);
            return slot;
        }

        private InteractionCard CreateCandidateCard(CardUpgradePresenter.UpgradeData data, CardAnchor anchor)
        {
            var card = SpawnCard(anchor, data.VisualInput);
            if (!card)
            {
                return null;
            }

            _cardMap.Add(card, data.UpgradeNode);
            _cardBaseScales[card] = card.transform.localScale;
            return card;
        }

        private InteractionCard SpawnCard(CardAnchor anchor, CardVisualInput visualInput)
        {
            if (!anchor)
            {
                Debug.LogError($"{nameof(CardUpgradeView)}: Card anchor is missing.", this);
                return null;
            }

            var card = anchor.Spawn(visualInput, transform);
            if (!card)
            {
                Debug.LogError($"{nameof(CardUpgradeView)}: Failed to spawn card from anchor '{anchor.name}'.", this);
            }

            return card;
        }

        private void SetCardSelectedVisual(InteractionCard card, bool isSelected)
        {
            if (!card)
            {
                return;
            }

            if (!_cardBaseScales.TryGetValue(card, out Vector3 baseScale))
            {
                baseScale = card.transform.localScale;
                _cardBaseScales[card] = baseScale;
            }

            Vector3 targetScale = isSelected ? baseScale * SelectedCardScaleMultiplier : baseScale;
            card.transform.DOKill();
            card.transform.DOScale(targetScale, SelectionTweenDuration).SetEase(Ease.OutQuad);
        }

        private void CreateClickArea(CandidateSlot slot)
        {
            if (slot == null || !slot.Card || !controlsCanvas)
            {
                return;
            }

            var canvasRect = controlsCanvas.GetComponent<RectTransform>();
            if (!canvasRect)
            {
                return;
            }

            var go = new GameObject($"Click Area - {slot.Card.name}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvasRect, false);

            var rect = (RectTransform)go.transform;
            Rect localRect = GetSlotLocalRect(slot, canvasRect);
            localRect.xMin -= SlotClickPaddingPx;
            localRect.xMax += SlotClickPaddingPx;
            localRect.yMin -= SlotClickPaddingPx;
            localRect.yMax += SlotClickPaddingPx;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = localRect.center;
            rect.sizeDelta = localRect.size;
            rect.SetAsLastSibling();

            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            slot.ClickAction = () => HandleCardSelected(slot.Card);
            button.onClick.AddListener(slot.ClickAction);

            slot.ClickArea = rect;
            slot.ClickButton = button;
        }

        private Rect GetSlotLocalRect(CandidateSlot slot, RectTransform canvasRect)
        {
            Rect cardRect = GetCardLocalRect(slot.Card, canvasRect);
            if (slot.TooltipPanel)
            {
                Rect tooltipRect = GetRectTransformLocalRect(slot.TooltipPanel.RectTransform, canvasRect, controlsCanvas.worldCamera);
                return Union(cardRect, tooltipRect);
            }

            return cardRect;
        }

        private Rect GetCardLocalRect(InteractionCard card, RectTransform canvasRect)
        {
            var renderers = card.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Vector2 center = WorldToCanvasLocal(card.transform.position, canvasRect);
                return new Rect(center - FallbackCardClickSizePx * 0.5f, FallbackCardClickSizePx);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 centerZ = new(0f, 0f, bounds.center.z);
            Vector2 p0 = WorldToCanvasLocal(new Vector3(min.x, min.y, centerZ.z), canvasRect);
            Vector2 p1 = WorldToCanvasLocal(new Vector3(min.x, max.y, centerZ.z), canvasRect);
            Vector2 p2 = WorldToCanvasLocal(new Vector3(max.x, min.y, centerZ.z), canvasRect);
            Vector2 p3 = WorldToCanvasLocal(new Vector3(max.x, max.y, centerZ.z), canvasRect);
            return RectFromPoints(p0, p1, p2, p3);
        }

        private Vector2 WorldToCanvasLocal(Vector3 worldPosition, RectTransform canvasRect)
        {
            var worldCamera = FindWorldCamera();
            if (!worldCamera)
            {
                return Vector2.zero;
            }

            Vector2 screenPoint = worldCamera.WorldToScreenPoint(worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPoint, controlsCanvas.worldCamera, out Vector2 localPoint);
            return localPoint;
        }

        private static Rect GetRectTransformLocalRect(RectTransform target, RectTransform canvasRect, Camera canvasCamera)
        {
            Vector3[] worldCorners = new Vector3[4];
            target.GetWorldCorners(worldCorners);

            Vector2[] localCorners = new Vector2[4];
            for (int i = 0; i < worldCorners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, worldCorners[i]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPoint, canvasCamera, out localCorners[i]);
            }

            return RectFromPoints(localCorners);
        }

        private static Rect RectFromPoints(params Vector2[] points)
        {
            Vector2 min = points[0];
            Vector2 max = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Union(Rect a, Rect b)
        {
            return Rect.MinMaxRect(
                Mathf.Min(a.xMin, b.xMin),
                Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax),
                Mathf.Max(a.yMax, b.yMax));
        }

        private static CardAnchor ResolveAnchor(Transform anchorTransform)
        {
            return anchorTransform ? anchorTransform.GetComponent<CardAnchor>() : null;
        }

        private void HideAllArrows()
        {
            SetArrowVisible(arrow_1_0, false);
            SetArrowVisible(arrow_2_0, false);
            SetArrowVisible(arrow_2_1, false);
        }

        private static void SetArrowVisible(GameObject arrow, bool visible)
        {
            if (arrow)
            {
                arrow.SetActive(visible);
            }
        }

        private Camera FindWorldCamera()
        {
            return transform.root.GetComponentInChildren<Camera>(true);
        }

        private void ClearCards()
        {
            foreach (var card in _cardMap.Keys)
            {
                if (!card)
                {
                    continue;
                }

                card.PointerUp -= HandleCardSelected;
                Destroy(card.gameObject);
            }

            foreach (var slot in _candidateSlots)
            {
                if (slot.ClickButton != null && slot.ClickAction != null)
                {
                    slot.ClickButton.onClick.RemoveListener(slot.ClickAction);
                }

                if (slot.ClickArea)
                {
                    Destroy(slot.ClickArea.gameObject);
                }
            }

            _cardMap.Clear();
            _cardBaseScales.Clear();
            _candidateSlots.Clear();

            if (_originalCard)
            {
                Destroy(_originalCard.gameObject);
            }

            HideAllArrows();
            _originalCard = null;
            _currentSelectedCard = null;
        }

        private void SetInteractable(bool value)
        {
            if (!canvasGroup)
            {
                return;
            }

            canvasGroup.blocksRaycasts = value;
            canvasGroup.interactable = value;
        }
    }
}
