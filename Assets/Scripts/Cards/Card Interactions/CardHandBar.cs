using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Systems;
using Cardevil.Events;
using TMPro;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardHandBar : MonoBehaviour, ICardHandBar, ITurnPlayerInput
    {
        [Header("InStage Data")]
        [SerializeField] InStageCards _stageCards;
        [SerializeField] CardContext _context;

        public InStageCards StageCards => _stageCards;
        public CardContext Context => _context;

        [Header("SO")]
        public BaseDeckConfiguration baseDeckConfig;
        public BaseDeckConfiguration baseRuntimeDeckConfig;
        [SerializeField] MultiplyValues multiplyValues;

        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public Card draggedCard { get; private set; }
        public SelectContainer selectContainer;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots = new Transform[6];

        [Header("References")]
        [SerializeField] Button useCardButton;
        [SerializeField] Button discardCardButton;
        [SerializeField] TextMeshProUGUI selectResultText;

        [Header("Setting")]
        [SerializeField] int initialCardCount = 6;
        [SerializeField] float selectOffset = 35f;
        [SerializeField] float drawInterval = .2f;
        [SerializeField] float discardInterval = .3f;

        [Header("State")]
        public bool CanInput => CanInteraction && !IsSwapping;

        // 카드 관련 상호작용 가능한가?
        private bool _canInteraction = false; 
        private bool CanInteraction
        {
            get => _canInteraction;
            set
            {
                _canInteraction = value;
                UpdateButtons();
            }
        }

        // 카드가 정렬, 소환 등 움직이고 있나?
        private bool _isSwapping = false; 
        private bool IsSwapping
        {
            get => _isSwapping;
            set
            {
                _isSwapping = value;
                UpdateButtons();
            }
        }



        public void Init()
        {
            CanInteraction = false;

            // TODO: CardManager에서 처리하게 하는게 더 옳을 듯
            DeckFactory.CreateRuntimeDeck(baseDeckConfig, baseRuntimeDeckConfig);

            _stageCards = new(DeckFactory.CreateStageDeck(baseRuntimeDeckConfig));
            _context = new(multiplyValues);

            for (int i = 0; i < initialCardCount; i++)
            {
                slots[i] = Instantiate(original: cardSlotPrefab, parent: transform).transform;
                slots[i].gameObject.SetActive(false);
            }

            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);

            // FIXME: EventManager보다 CardManager가 먼저 Init돼서 실행이 안됨
            UpdateDeckCardCount();
        }

        void Update()
        {
            if (!CanInput)
                return;
            if (draggedCard == null)
                return;
            DetectSwap();
        }



        public void Select(Card card)
        {
            StageCards.Select(card);
            UpdateButtons();
        }

        public void Deselect(Card card)
        {
            StageCards.Deselect(card);
            UpdateButtons();
        }

        // 플레이어 턴이면서 카드 값 선택이 바뀔 때.
        // Card.onselectEnded에서 호출
        public void OnSelectValueEnd(Card _)
        {
            UpdateButtons();
        }


        #region Card Event
        private void BeginDrag(Card card)
        {
            draggedCard = card;
        }

        private void EndDrag(Card card)
        {
            if (draggedCard == null)
                return;

            draggedCard.transform.DOLocalMove(
                endValue: draggedCard.isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero,
                duration: .2f
            )
            .SetEase(Ease.OutBack);

            draggedCard = null;
        }
        #endregion


        #region Swap
        private void DetectSwap()
        {
            for (int i = 0; i < StageCards.HandCount; i++)
            {
                if (draggedCard.transform.position.x > StageCards.GetCard(i).transform.position.x)
                    if (draggedCard.GetSlotIndex() < StageCards.GetCard(i).GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }

                if (draggedCard.transform.position.x < StageCards.GetCard(i).transform.position.x)
                    if (draggedCard.GetSlotIndex() > StageCards.GetCard(i).GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            var swappedCard = StageCards.GetCard(index);
            var selectedCardSlot = draggedCard.transform.parent;
            var swappedSlot = swappedCard.transform.parent;

            swappedCard.transform.SetParent(selectedCardSlot);
            swappedCard.transform.localPosition = swappedCard.isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero;

            draggedCard.transform.SetParent(swappedSlot);

            StageCards.UpdateVisualIndex();
            
            IsSwapping = false;
        }
        #endregion


        #region Use & Discard & Spawn
        
        private void Use()
        {
            Context.GetSet();
            CardResultEvaluator.Evaluate(Context, StageCards.Selects);
            _ = DiscardAsync();
            cmp.TrySetResult();
        }

        private void Discard()
        {
            _ = DiscardAndDrawAsync();
        }

        private Card Spawn(int slotIndex)
        {
            var cardData = StageCards.DrawCard();
            if (cardData == null)
                return null;

            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectValueEndEvent += OnSelectValueEnd;

            StageCards.Draw(card);
            UpdateDeckCardCount();

            // FIXME: 애니메이션의 순서 제어를 위해
            // 실행 위치 혹은 방식을 바꿔야할 듯!
            StageCards.UpdateVisualIndex();

            return card;
        }

        public async UniTask DrawAsync()
        {
            IsSwapping = true;

            var inactiveSlots = slots.Where(s => !s.gameObject.activeSelf)
                        .ToArray();

            foreach (var slot in inactiveSlots)
            {
                var slotIndex = slot.GetSiblingIndex();
                slot.gameObject.SetActive(true);
                Spawn(slotIndex);
                await UniTask.Delay(TimeSpan.FromSeconds(drawInterval));
            }

            IsSwapping = false;
        }

        public async UniTask DiscardAsync()
        {
            IsSwapping = true;

            await StageCards.Discard(discardInterval, slots);

            // slot 정렬
            // - - - - - -
            var inactiveSlots = slots.Where(s => !s.gameObject.activeSelf)
                        .ToArray();

            foreach (var slot in inactiveSlots)
            {
                slot.SetSiblingIndex(2);
            }

            slots = slots
                    .OrderBy(t => t.GetSiblingIndex())
                    .ToArray();

            IsSwapping = false;
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
            using (var args = RemainingCardChangeArgs.Get())
            {
                args.Init(StageCards.DeckCount);
                Managers.Event.RemainingCardChangeEvent?.Invoke(args);
            }
        }

        private void UpdateButtons()
        {
            if (CanInput && StageCards.SelectCount > 0 && StageCards.AllValueSelected)
                selectResultText.text = CardResultEvaluator.CheckResult(Context, StageCards.Selects).Description;
            else
                selectResultText.text = "";

            var canUse = CanInput && StageCards.CanUseCard;
            useCardButton.interactable = canUse;

            var canDiscard = CanInput && StageCards.SelectCount > 0;
            discardCardButton.interactable = canDiscard;
        }



        #region IUserInput
        public bool IsNoCard => false;

        public void ActivateInteraction()
        {
            cmp = new();
            CanInteraction = true;
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
        #endregion
    }
}
