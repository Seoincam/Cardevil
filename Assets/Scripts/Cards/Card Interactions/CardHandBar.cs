using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Systems;
using TMPro;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardHandBar : MonoBehaviour, ICardHandBar, ITurnPlayerInput
    {
        [Header("InStage Data")]
        [SerializeField] StageCardsContext stageCardsCtx;
        [SerializeField] CardResultContext resultCtx;
        [SerializeField] int _maxCardCount = 6;

        [Header("SO")]
        [SerializeField] MultiplyValues multiplyValues;

        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        [SerializeField] Card _draggedCard;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots;

        [Header("Reroll")]
        [SerializeField] GameObject rerollPanel;
        [SerializeField] Transform rerollBar;
        [SerializeField] Button rerollSelectButton;
        [SerializeField] Button rerollButton;
        private Transform[] rerollSlots;

        [Header("UI")]
        public SelectContainer selectContainer;
        [SerializeField] Button useCardButton;
        [SerializeField] Button discardCardButton;
        [SerializeField] Button sortByNumberButton;
        [SerializeField] Button sortByIconButton;
        [SerializeField] TextMeshProUGUI selectResultText;
        [SerializeField] TextMeshProUGUI deckCountText;
        [SerializeField] GameObject dummyCardVisual;
        [SerializeField] RectTransform deckRect;

        [Space, SerializeField] BlueFlushChoice blueFlushChoice;

        [Header("Setting")]
        [SerializeField] int initialCardCount = 6;
        [SerializeField] float selectOffset = 35f;
        [SerializeField] float drawInterval = .2f;
        [SerializeField] float discardInterval = .3f;
        [SerializeField] float reviveInterval = .4f;

        // 카드 관련 상호작용 가능한가?
        private bool _canInteraction = false;

        // 카드가 정렬, 소환 등 움직이고 있나?
        private bool _isSwapping = false;

        public StageCardsContext StageCardsCtx => stageCardsCtx;

        public CardResultContext Context => resultCtx;

        public int MaxCardCount => _maxCardCount;

        public bool CanInput => CanInteraction && !IsSwapping;

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


        public void Init()
        {
            CanInteraction = false;

            stageCardsCtx = new(DeckFactory.CreateStageDeck(Managers.Card.runtimeBaseDeck));
            resultCtx = new(multiplyValues);

            slots = new Transform[initialCardCount];
            for (int i = 0; i < initialCardCount; i++)
                slots[i] = Instantiate(original: cardSlotPrefab, parent: transform).transform;

            rerollCmp = new();
            rerollSlots = new Transform[initialCardCount];
            for (int i = 0; i < initialCardCount; i++)
                rerollSlots[i] = Instantiate(original: cardSlotPrefab, parent: rerollBar).transform;

            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);
            sortByNumberButton.onClick.AddListener(SortByNumber);
            sortByIconButton.onClick.AddListener(SortByIcon);

            UpdateDeckCardCount();

            StartReroll();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _ = Revive(3);
            }

            if (!CanInput)
                return;
            if (DraggedCard == null)
                return;
            DetectSwap();
        }



        public void Select(Card card)
        {
            StageCardsCtx.Select(card);
            UpdateUI();
        }

        public void Deselect(Card card)
        {
            StageCardsCtx.Deselect(card);
            UpdateUI();
        }

        // 플레이어 턴이면서 카드 값 선택이 바뀔 때.
        // Card.onselectEnded에서 호출
        public void OnSelectValueEnd(Card _)
        {
            UpdateUI();
        }


        #region Card Event
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
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero,
                duration: .2f
            )
            .SetEase(Ease.OutBack);

            DraggedCard = null;
        }
        #endregion

        private async UniTask RerollAsync()
        {
            foreach (var card in StageCardsCtx.Hand)
            {
                StageCardsCtx.Discard(card, discardInterval);
                await UniTask.Delay(TimeSpan.FromSeconds(discardInterval));
                card.Destroy(); // TODO: 이벤트 구독 해지 로직/오브젝트 풀 관련 로직 추가
                UpdateSlot();
            }
        }


        #region Swap
        private void DetectSwap()
        {
            for (int i = 0; i < StageCardsCtx.HandCount; i++)
            {
                if (DraggedCard.transform.position.x > StageCardsCtx.GetHandCard(i).transform.position.x)
                    if (DraggedCard.HandIndex < StageCardsCtx.GetHandCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }

                if (DraggedCard.transform.position.x < StageCardsCtx.GetHandCard(i).transform.position.x)
                    if (DraggedCard.HandIndex > StageCardsCtx.GetHandCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            StageCardsCtx.Swap(DraggedCard.HandIndex, index);
            UpdateSlot();

            IsSwapping = false;
        }

        public void UpdateSlot(bool isReroll = false)
        {
            if (isReroll)
            {
                foreach (var card in stageCardsCtx.Hand)
                    card.SetSlot(rerollSlots[card.HandIndex], isDragging: false);
            }
            else
            {
                foreach (var card in StageCardsCtx.Hand)
                    card.SetSlot(slots[card.HandIndex], isDragging: card == DraggedCard);
            }
        }

        #endregion


        #region Use & Discard & Spawn

        private void Use()
        {
            CardResultEvaluator.SetResult(Context, StageCardsCtx.Selects);
            _ = UseAsync();
        }

        private void Discard()
        {
            // TODO: 버리기 횟수 0되면 못 버리게
            StageCardsCtx.ReduceDiscardCount();
            _ = DiscardAndDrawAsync();
        }

        private Card Spawn(bool isReroll = false)
        {
            var cardData = StageCardsCtx.DrawCard();
            if (cardData == null)
                return null;

            Card card = Instantiate(original: cardPrefab).GetComponent<Card>();
            card.Init(StageCardsCtx, barGroup: this, cardData);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectValueEndEvent += OnSelectValueEnd;

            StageCardsCtx.Draw(card);
            UpdateDeckCardCount();
            UpdateSlot(isReroll);
            return card;
        }

        private async UniTask UseAsync()
        {
            await DiscardAsync();

            if (Context.CurrentResult.IsBlueFlush)
            {
                blueFlushChoice.GetSet(this);
                await blueFlushChoice.BlueFlushCmp.Task;
            }
            else if (Context.CurrentResult.IsGreenFlush)
            {
                Managers.Game.PlayerStatus.Shield += 1;
            }
            else if (Context.CurrentResult.IsBlackFlush)
            {
                Context.SetBlackFlushUsed();
                // 최대 HP 1로 고정
            }

            cmp.TrySetResult();
        }

        private async UniTask DrawAsync(bool isReroll = false)
        {
            IsSwapping = true;
            var count = MaxCardCount - StageCardsCtx.HandCount;
            for (int i = 0; i < count; i++)
            {
                Spawn(isReroll);
                await UniTask.Delay(TimeSpan.FromSeconds(drawInterval));
            }

            IsSwapping = false;
        }

        private async UniTask DiscardAsync()
        {
            foreach (var card in StageCardsCtx.SortedSelect)
            {
                StageCardsCtx.Discard(card, discardInterval);
                await UniTask.Delay(TimeSpan.FromSeconds(discardInterval));
                card.Destroy(); // TODO: 이벤트 구독 해지 로직/오브젝트 풀 관련 로직 추가
                UpdateSlot();
            }

            StageCardsCtx.Selects.Clear();
        }

        private async UniTask DiscardAndDrawAsync()
        {
            IsSwapping = true;
            await DiscardAsync();
            await DrawAsync();
            IsSwapping = false;
        }

        #endregion

        private void UpdateDeckCardCount()
        {
            deckCountText.text = StageCardsCtx.DeckCount.ToString();
        }

        private void SortByNumber()
        {
            StageCardsCtx.SortByNumber();
            UpdateUI();
            UpdateSlot();
        }

        private void SortByIcon()
        {
            StageCardsCtx.SortByIcon();
            UpdateUI();
            UpdateSlot();
        }

        private void UpdateUI()
        {
            if (CanInput && StageCardsCtx.CanUseCard)
                selectResultText.text = CardResultEvaluator.Evaluate(Context, StageCardsCtx.Selects).Description;
            else
                selectResultText.text = "";

            var canUse = CanInput && StageCardsCtx.CanUseCard && !DraggedCard;
            useCardButton.interactable = canUse;
            UpdateDeckCardCount();

            var canDiscard = CanInput && StageCardsCtx.SelectCount > 0 && !DraggedCard;
            discardCardButton.interactable = canDiscard;
            discardCardButton.GetComponentInChildren<Text>().text = $"버리기 ({StageCardsCtx.DiscardRemainCount})";
        }

        public async UniTask Revive(int amount)
        {
            amount = Mathf.Min(amount, StageCardsCtx.DiscardCount);
            for (int i = 0; i < amount; i++)
            {
                var dummyCard = Instantiate(dummyCardVisual, parent: deckRect.transform);
                dummyCard.transform.SetSiblingIndex(1);
                var tween = dummyCard.transform.DOLocalMove(new Vector3(0, 0), reviveInterval)
                                            .SetEase(Ease.OutCubic);
                await tween.AsyncWaitForCompletion();
                Destroy(dummyCard);
                StageCardsCtx.Revive();
                UpdateDeckCardCount();
            }
        }



        #region IUserInput
        public bool IsNoCard => false;

        public void ActivateInteraction()
        {
            cmp = new();
            CanInteraction = true;
            Context.GetSet();
        }

        public void InactivateInteraction()
        {
            CanInteraction = false;
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

        private UniTaskCompletionSource rerollCmp;
        public async UniTask RerollCard()
        {
            await rerollCmp.Task;
        }
        #endregion

        // reroll
        private void StartReroll()
        {   
            rerollSelectButton.onClick.AddListener(SelectReroll);
            rerollButton.onClick.AddListener(Reroll);
            _ = DrawAsync(isReroll: true);
        }

        private void Reroll()
        {
            foreach (var card in stageCardsCtx.Hand)
            {
                // TODO: 다시 덱으로 돌아가는 tween
                card.Destroy();
            }
            
            // TODO: Reroll권 감소
            stageCardsCtx = new(DeckFactory.CreateStageDeck(Managers.Card.runtimeBaseDeck));
            _ = DrawAsync(isReroll: true);
        }

        private void SelectReroll()
        {
            UpdateSlot();
            rerollPanel.SetActive(false);
            rerollCmp.TrySetResult();
        }
    }
}
