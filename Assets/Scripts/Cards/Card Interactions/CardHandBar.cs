using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Systems;
using Cardevil.Events;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardHandBar : MonoBehaviour, ICardHandBar, ITurnPlayerInput
    {
        public InGameDeck Deck { get; private set; }
        public InGameHand Hand { get; private set; }
        public CardContext Context => _context;

        private CardContext _context;

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



        void Update()
        {
            if (!CanInput)
                return;
            if (draggedCard == null)
                return;
            DetectSwap();
        }



        public void Init()
        {
            CanInteraction = false;

            DeckFactory.InitRuntimeDeckConfig(baseDeckConfig, baseRuntimeDeckConfig);
            Deck = new(baseRuntimeDeckConfig);
            Hand = new();
            _context = new(multiplyValues);

            for (int i = 0; i < initialCardCount; i++)
            {
                var slot = Instantiate(original: cardSlotPrefab, parent: transform);
                slot.gameObject.SetActive(false);
                slots[i] = slot.transform;
            }

            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);

            // FIXME: EventManager보다 CardManager가 먼저 Init돼서 실행이 안됨
            UpdateDeckCardCount();
        }

        public void AddSelectedCard(Card card)
        {
            Hand.Select(card);
            UpdateButtons();
        }

        public void RemoveSelectedCard(Card card)
        {
            Hand.Deselect(card);
            UpdateButtons();
        }

        // 플레이어 턴이면서 카드 값 선택이 바뀔 때, -> Card.onselectEnded에서 호출
        public void OnSelectValueEnd(Card _)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            var canUse = CanInput && Hand.SelectCount > 0 && Hand.AllValueSelected;
            useCardButton.interactable = canUse;

            var canDiscard = CanInput && Hand.SelectCount > 0;
            discardCardButton.interactable = canDiscard;
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
            for (int i = 0; i < Hand.HandCount; i++)
            {
                if (draggedCard.transform.position.x > Hand.GetCard(i).transform.position.x)
                    if (draggedCard.GetSlotIndex() < Hand.GetCard(i).GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }

                if (draggedCard.transform.position.x < Hand.GetCard(i).transform.position.x)
                    if (draggedCard.GetSlotIndex() > Hand.GetCard(i).GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            var swappedCard = Hand.GetCard(index);
            var selectedCardSlot = draggedCard.transform.parent;
            var swappedSlot = swappedCard.transform.parent;

            swappedCard.transform.SetParent(selectedCardSlot);
            swappedCard.transform.localPosition = swappedCard.isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero;

            draggedCard.transform.SetParent(swappedSlot);

            draggedCard.cardVisual.UpdateIndex(draggedCard.GetSlotIndex());
            swappedCard.cardVisual.UpdateIndex(swappedCard.GetSlotIndex());
            
            IsSwapping = false;
        }

        #endregion


        #region Use & Discard & Spawn
        
        private void Use()
        {
            CardResultEvaluator.Evaluate(Context, Hand.Selects);
            _ = DiscardAsync();
            cmp.TrySetResult();
        }

        private void Discard()
        {
            _ = DiscardAndDrawAsync();
        }

        private Card Spawn(int slotIndex)
        {
            var cardData = Deck.DrawCard();
            if (cardData == null)
                return null;

            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectValueEndEvent += OnSelectValueEnd;

            Hand.Draw(card);
            UpdateDeckCardCount();

            // FIXME: 애니메이션의 순서 제어를 위해
            // 실행 위치 혹은 방식을 바꿔야할 듯!
            Hand.UpdateVisualIndex();

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

            await Hand.Discard(discardInterval, slots);

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
                args.Init(Deck.Count);
                Managers.Event.RemainingCardChangeEvent?.Invoke(args);
            }
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
