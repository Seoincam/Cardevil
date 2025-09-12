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

        [Header("SO")]
        [SerializeField] MultiplyValues multiplyValues;
        [SerializeField] CardVisualSetting visualSetting;

        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        [SerializeField] Card _draggedCard;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots;

        [Header("Reroll")]
        [SerializeField] Image rerollPanel;
        [SerializeField] Transform rerollBar;
        [SerializeField] Button rerollSelectButton;
        [SerializeField] Button rerollButton;
        [SerializeField] Button toggleInGamePreviewButton;
        [SerializeField] GameObject cardVisualHandler;
        private Transform[] rerollSlots;
        private bool isPreviewInGame;

        [Header("UI")]
        public SelectContainer selectContainer;
        [SerializeField] Button useCardButton;
        [SerializeField] Button discardCardButton;
        [SerializeField] Button sortByNumberButton;
        [SerializeField] Button sortByIconButton;
        [SerializeField] TextMeshProUGUI selectResultText;
        [SerializeField] TextMeshProUGUI deckCountText;
        [SerializeField] GameObject dummyCardVisual;

        [Space, SerializeField] BlueFlushChoice blueFlushChoice;
        public CardDeckVisual deck;
        

        // 카드 관련 상호작용 가능한가?
        private bool _canInteraction = false;

        // 카드가 정렬, 소환 등 움직이고 있나?
        private bool _isSwapping = false;

        public StageCardsContext StageCardsCtx => stageCardsCtx;

        public CardResultContext Context => resultCtx;

        

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


        public void Init()
        {
            CanInteraction = false;

            // Init Contexts
            stageCardsCtx = new(DeckFactory.CreateStageDeck(Managers.Card.runtimeBaseDeck));
            resultCtx = new(multiplyValues);

            // Init Slots
            slots = new Transform[Managers.Card.MaxCardCount];
            rerollSlots = new Transform[Managers.Card.MaxCardCount];
            for (int i = 0; i < Managers.Card.MaxCardCount; i++)
            {
                if (slots[i] == null)
                    slots[i] = Instantiate(original: cardSlotPrefab, parent: transform).transform;
                if (rerollSlots[i] == null)
                    rerollSlots[i] = Instantiate(original: cardSlotPrefab, parent: rerollBar).transform;
            }                

            // Init Buttons
            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);
            sortByNumberButton.onClick.AddListener(SortByNumber);
            sortByIconButton.onClick.AddListener(SortByIcon);

            visualSetting.SetDeckVisual(deck);

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


        // Reroll
        // - - - - - - - - - - -
        private void InitRerollPanel()
        {   
            rerollSelectButton.onClick.AddListener(EndReroll);
            rerollButton.onClick.AddListener(Reroll);
            toggleInGamePreviewButton.onClick.AddListener(ToggleStagePreview);

            rerollPanel.gameObject.SetActive(true);
            toggleInGamePreviewButton.gameObject.SetActive(true);
            deckCountText.gameObject.SetActive(false);

            _ = RerollAsync();
        }

        private async UniTask RerollAsync()
        {
            rerollButton.interactable = false;
            rerollSelectButton.interactable = false;
            toggleInGamePreviewButton.interactable = false;

            foreach (var card in stageCardsCtx.Hand)
            {
                card.OnRerollDiscard?.Invoke(deck.Front);
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.RerollDiscardInterval));
            }

            stageCardsCtx = new(DeckFactory.CreateStageDeck(Managers.Card.runtimeBaseDeck));

            await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.RerollDrawDiscardInterval));

            for (int i = 0; i < Managers.Card.MaxCardCount; i++)
            {
                Spawn(isReroll: true).OnRerollDraw?.Invoke();
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.RerollDrawInterval));
            }

            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            rerollButton.interactable = true;
            rerollSelectButton.interactable = true;
            toggleInGamePreviewButton.interactable = true;
        }

        // 카드를 버리고 다시 뽑음.
        private void Reroll()
        {
            // TODO: Reroll권 감소
            _ = RerollAsync();
        }

        // 카드 선택을 끝냄.
        private void EndReroll()
        {
            UpdateSlot();

            rerollPanel.gameObject.SetActive(false);
            toggleInGamePreviewButton.gameObject.SetActive(false);
            deckCountText.gameObject.SetActive(true);

            rerollCmp.TrySetResult();
        }

        private void ToggleStagePreview()
        {
            isPreviewInGame = !isPreviewInGame;

            if (isPreviewInGame)
            {
                toggleInGamePreviewButton.GetComponentInChildren<Text>().text = "카드 선택하기";
                rerollPanel.color = new Color(1, 1, 1, 0);
            }
            else
            {
                toggleInGamePreviewButton.GetComponentInChildren<Text>().text = "인게임 미리 보기";
                rerollPanel.color = new Color(0, 0, 0, .85f);
            }

            rerollButton.gameObject.SetActive(!isPreviewInGame);
            rerollSelectButton.gameObject.SetActive(!isPreviewInGame);
            cardVisualHandler.SetActive(!isPreviewInGame);
        }


        // Select
        // - - - - - - - - - - -
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
                foreach (var card in stageCardsCtx.Hand)
                {
                    card.SetSlot(rerollSlots[card.HandIndex], isDragging: false);
                    card.SetReroll(true);
                }
                    
            else
                foreach (var card in StageCardsCtx.Hand)
                {
                    card.SetSlot(slots[card.HandIndex], isDragging: card == DraggedCard);
                    card.SetReroll(false);
                }
                    
        }


        // Use & Discard
        // - - - - - - - - - - -
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

        private async UniTask DiscardAsync()
        {
            foreach (var card in StageCardsCtx.SortedSelect)
            {
                StageCardsCtx.Discard(card);
                await UniTask.Delay(TimeSpan.FromSeconds(visualSetting.DiscardInterval));
                UpdateSlot();
            }

            StageCardsCtx.Selects.Clear();
        }

        private async UniTask DrawAsync()
        {
            IsSwapping = true;
            var count = Managers.Card.MaxCardCount - StageCardsCtx.HandCount;
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


        // Sort
        // - - - - - - - - - - -
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


        // UI
        // - - - - - - - - - - -
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

        private void UpdateDeckCardCount()
        {
            deckCountText.text = $"{StageCardsCtx.DeckCount} / 50";
        }


        public async UniTask Revive(int amount)
        {
            amount = Mathf.Min(amount, StageCardsCtx.DiscardCount);
            for (int i = 0; i < amount; i++)
            {
                var dummyCard = Instantiate(dummyCardVisual, parent: visualSetting.deck.Front.transform);
                dummyCard.transform.SetSiblingIndex(1);
                var tween = dummyCard.transform.DOLocalMove(new Vector3(0, 0), visualSetting.ReviveInterval)
                                            .SetEase(Ease.OutCubic);
                await tween.AsyncWaitForCompletion();
                Destroy(dummyCard);
                StageCardsCtx.Revive();
                UpdateDeckCardCount();
            }
        }



        // Turn Interface UserInput
        // - - - - - - - - - - -
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
            rerollCmp = new();

            // 보스 공격 범위 보여주고 대기
            await UniTask.Delay(TimeSpan.FromSeconds(.75f));
            InitRerollPanel();
            await rerollCmp.Task;
        }
    }
}
