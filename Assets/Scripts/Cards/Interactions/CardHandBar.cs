using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Systems;
using TMPro;
using System.Collections.Generic;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Attributes;

namespace Cardevil.Cards.Interactions
{
    public class CardHandBar : MonoBehaviour, ICardHandBar, ITurnPlayerInput, IClearable
    {
        private CardManager _manager;
        private StageCardsContext ctx;

        [Header("SO")]
        [SerializeField] CardVisualSettingSO visualSetting;

        [Header("Card")]
        [SerializeField, VisibleOnly] Card _draggedCard;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        [SerializeField] private List<Transform> slots = new();

        [Header("UI")]
        public SelectContainer selectContainer;
        [SerializeField] Button useCardButton;
        [SerializeField] Button discardCardButton;
        [SerializeField] Button sortByNumberButton;
        [SerializeField] Button sortByIconButton;
        [SerializeField] Text selectResultText;
        [SerializeField] TextMeshProUGUI deckCountText;
        [SerializeField] GameObject dummyCardVisual;

        [Space, SerializeField] BlueFlushChoice blueFlushChoice;
        [SerializeField] GameObject cardVisualHandler;

        public CardDeckVisual deck;
        

        // 카드 관련 상호작용 가능한가?
        [SerializeField] private bool _canInteraction = false;

        // 카드가 정렬, 소환 등 움직이고 있나?
        [SerializeField] private bool _isSwapping = false;
        

        public bool CanInput
        {
            get => CanInteraction && !IsSwapping;
        }

        public Card DraggedCard
        {
            get => _draggedCard;
            private set
            {
                _draggedCard = value;
                UpdateUI();
            }
        }

        private bool IsSwapping
        {
            get => _isSwapping;
            set
            {
                _isSwapping = value;
                UpdateUI();
            }
        }

        private bool CanInteraction
        {
            get => _canInteraction;
            set
            {
                _canInteraction = value;
                UpdateUI();
            }
        }

        public void Init(CardManager manager, StageCardsContext ctx)
        {
            _manager = manager;
            this.ctx = ctx;

            // Init Buttons
            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);
            sortByNumberButton.onClick.AddListener(SortByNumber);
            sortByIconButton.onClick.AddListener(SortByIcon);
        }

        public void Clear()
        {
            CanInteraction = false;

            foreach (var slot in slots)
            {
                foreach (Transform child in slot)
                    Destroy(child.gameObject);
            }
            if (slots.Count < _manager.MaxHandCount)
            {
                for (int i = 0; i < _manager.MaxHandCount; i++)
                    slots.Add(Instantiate(original: cardSlotPrefab, transform).transform);                   
            }

            UpdateDeckCardCount();
        }

        void Update()
        {
            if (!CanInput)
                return;
            if (DraggedCard == null)
                return;
            DetectSwap();
        }


        // Select
        // - - - - - - - - - - -
        public void Select(Card card)
        {
            _manager.StageCardsCtx.Select(card);
            UpdateUI();
        }

        public void Deselect(Card card)
        {
            _manager.StageCardsCtx.Deselect(card);
            UpdateUI();
        }

        // 플레이어 턴이면서 카드 값 선택이 바뀔 때.
        // Card.onselectEnded에서 호출
        public void OnSelectValueEnd(Card _)
        {
            UpdateUI();
        }



        // Card Event
        // - - - - - - - - - - -
        private void BeginDrag(Card card)
        {
            DraggedCard = card;
        }

        private void EndDrag(Card card)
        {
            if (DraggedCard == null)
                return;

            DraggedCard.transform.DOLocalMove(
                endValue: DraggedCard.isSelected
                    ? new Vector3(0, visualSetting.SelectOffset, 0)
                    : Vector3.zero,
                duration: visualSetting.EndDragTweenDuration)
            .SetEase(Ease.OutBack);

            DraggedCard = null;
        }



        // Swap
        // - - - - - - - - - - -
        private void DetectSwap()
        {
            for (int i = 0; i < ctx.HandCount; i++)
            {
                if (DraggedCard.transform.position.x > ctx.GetHandCard(i).transform.position.x)
                    if (DraggedCard.HandIndex < ctx.GetHandCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }

                if (DraggedCard.transform.position.x < ctx.GetHandCard(i).transform.position.x)
                    if (DraggedCard.HandIndex > ctx.GetHandCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            _manager.StageCardsCtx.Swap(DraggedCard.HandIndex, index);
            UpdateSlots();

            IsSwapping = false;
        }

        public void UpdateSlots()
        {
            foreach (var card in ctx.Hand)
            {
                card.SetSlot(slots[card.HandIndex], isDragging: card == DraggedCard);
                card.IsReroll = false;
            }          
        }

        public void MoveToHandBar(int index)
        {
            var card = ctx.GetHandCard(index);
            card.SetSlot(slots[index], isDragging: card == DraggedCard);
            card.SetHandBar(this);
            AddListeners(card);
        }

        private void AddListeners(Card card)
        {
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectValueEndEvent += OnSelectValueEnd;
        }


        // Use & Discard
        // - - - - - - - - - - -
        private void Use()
        {
            CanInteraction = false;
            CardResultEvaluator.PreEvaluate(ctx.Selects);
            _ = UseAsync();
        }

        private void Discard()
        {
            // TODO: 버리기 횟수 0되면 못 버리게
            ctx.ReduceDiscardCount();
            _ = DiscardAndDrawAsync();
        }

        private async UniTask UseAsync()
        {
            await _manager.Evaluations.InvokeAsync(selectResultText);
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            await DiscardAsync();

            cmp.TrySetResult();
        }

        private async UniTask DiscardAsync()
        {
            foreach (var card in ctx.SortedSelect)
            {
                ctx.Discard(card);
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DiscardInterval));
                UpdateSlots();
            }

            ctx.Selects.Clear();
        }

        private async UniTask DrawAsync()
        {
            IsSwapping = true;
            var count = Managers.Card.MaxHandCount - ctx.HandCount;
            for (int i = 0; i < count; i++)
            {
                Spawn().OnDraw?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DrawInterval));
            }

            IsSwapping = false;
        }

        private async UniTask DiscardAndDrawAsync()
        {
            IsSwapping = true;
            await DiscardAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DiscardDrawInterval));
            await DrawAsync();
            IsSwapping = false;
        }


        // Spawn
        // - - - - - - - - - - -
        private Card Spawn()
        {
            var cardData = ctx.PopCard();
            if (cardData == null)
                return null;

            var card = Managers.Resource.Instantiate("Cards/Card").GetComponent<Card>();
            card.Init(ctx, handBar: this, cardData);

            // 이벤트 구독
            AddListeners(card);

            ctx.Draw(card);
            UpdateDeckCardCount();
            UpdateSlots();

            return card;
        }


        // Sort
        // - - - - - - - - - - -
        private void SortByNumber()
        {
            ctx.SortByNumber();
            UpdateUI();
            UpdateSlots();
        }

        private void SortByIcon()
        {
            ctx.SortByIcon();
            UpdateUI();
            UpdateSlots();
        }


        // UI
        // - - - - - - - - - - -
        private void UpdateUI()
        {
            selectResultText.text = ctx.Description;

            var canUse = CanInput && ctx.CanUseCard && !DraggedCard;
            useCardButton.interactable = canUse;
            UpdateDeckCardCount();

            var canDiscard = CanInput && ctx.SelectCount > 0 && !DraggedCard;
            discardCardButton.interactable = canDiscard;
            discardCardButton.GetComponentInChildren<Text>().text = $"버리기 ({ctx.DiscardRemainCount})";
        }

        private void UpdateDeckCardCount()
        {
            deckCountText.text = $"{ctx.DeckCount} / 50";
        }


        public async UniTask Revive(int amount)
        {
            amount = Mathf.Min(amount, ctx.DiscardCount);
            for (int i = 0; i < amount; i++)
            {
                var dummyCard = Instantiate(dummyCardVisual, parent: deck.Front.transform);
                dummyCard.transform.SetSiblingIndex(1);
                var tween = dummyCard.transform.DOLocalMove(new Vector3(0, 0), visualSetting.ReviveInterval)
                                            .SetEase(Ease.OutCubic);
                await tween.AsyncWaitForCompletion();
                Destroy(dummyCard);
                ctx.Revive();
                UpdateDeckCardCount();
            }
        }



        // Turn Interface UserInput
        // - - - - - - - - - - -
        public bool IsNoCard => false;

        public void ActivateInteraction()
        {
            _manager.ResultCtx.StepToNext();
            cmp = new();
            CanInteraction = true;
        }

        public void InactivateInteraction()
        {
            CanInteraction = false;
            Managers.Card.ResultCtx.Push();
        }

        public async UniTask DrawCard()
        {
            await DrawAsync();
        }

        private UniTaskCompletionSource cmp;

        public async UniTask WaitUserInput()
        {
            await cmp.Task;
        }
    }
}
