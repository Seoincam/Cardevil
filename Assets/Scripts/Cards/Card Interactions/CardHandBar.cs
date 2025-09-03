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
        [SerializeField] int _maxCardCount = 6;

        public InStageCards StageCards => _stageCards;
        public CardContext Context => _context;
        public int MaxCardCount => _maxCardCount;

        [Header("SO")]
        [SerializeField] MultiplyValues multiplyValues;

        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public SelectContainer selectContainer;
        [SerializeField] Card _draggedCard;
        public Card DraggedCard
        {
            get => _draggedCard;
            private set
            {
                _draggedCard = value;
                UpdateUI();
            }
        }

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
                UpdateUI();
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
                UpdateUI();
            }
        }



        public void Init()
        {
            CanInteraction = false;

            // TODO: CardManager에서 처리하게 하는게 더 옳을 듯
            _stageCards = new(DeckFactory.CreateStageDeck(Managers.Card.runtimeBaseDeck));
            _context = new(multiplyValues);

            for (int i = 0; i < initialCardCount; i++)
                slots[i] = Instantiate(original: cardSlotPrefab, parent: transform).transform;

            useCardButton.onClick.AddListener(Use);
            discardCardButton.onClick.AddListener(Discard);

            // FIXME: EventManager보다 CardManager가 먼저 Init돼서 실행이 안됨
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



        public void Select(Card card)
        {
            StageCards.Select(card);
            UpdateUI();
        }

        public void Deselect(Card card)
        {
            StageCards.Deselect(card);
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


        #region Swap
        private void DetectSwap()
        {
            for (int i = 0; i < StageCards.HandCount; i++)
            {
                if (DraggedCard.transform.position.x > StageCards.GetCard(i).transform.position.x)
                    if (DraggedCard.HandIndex < StageCards.GetCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }

                if (DraggedCard.transform.position.x < StageCards.GetCard(i).transform.position.x)
                    if (DraggedCard.HandIndex > StageCards.GetCard(i).HandIndex)
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        private void Swap(int index)
        {
            IsSwapping = true;

            StageCards.Swap(DraggedCard.HandIndex, index);
            SetSlots();
            
            IsSwapping = false;
        }

        public void SetSlots()
        {
            foreach (var card in StageCards.Hands)
                card.SetSlot(slots[card.HandIndex], isDragging: card == DraggedCard);
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

        private Card Spawn()
        {
            var cardData = StageCards.DrawCard();
            if (cardData == null)
                return null;

            var card = Instantiate(original: cardPrefab, parent: slots[0]).GetComponent<Card>();
            card.Init(StageCards, barGroup: this, cardData);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectValueEndEvent += OnSelectValueEnd;

            StageCards.Draw(card);
            UpdateDeckCardCount();
            SetSlots();
            return card;
        }

        public async UniTask DrawAsync()
        {
            IsSwapping = true;
            var count = MaxCardCount - StageCards.HandCount;
            for (int i = 0; i < count; i++)
            {
                Spawn();
                await UniTask.Delay(TimeSpan.FromSeconds(drawInterval));
            }

            IsSwapping = false;
        }

        public async UniTask DiscardAsync()
        {
            IsSwapping = true;
            SetSlots();
            await StageCards.Discard(discardInterval, slots);
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

        private void UpdateUI()
        {
            if (CanInput && StageCards.SelectCount > 0 && StageCards.AllValueSelected)
                selectResultText.text = CardResultEvaluator.CheckResult(Context, StageCards.Selects).Description;
            else
                selectResultText.text = "";

            var canUse = CanInput && StageCards.CanUseCard && !DraggedCard;
            useCardButton.interactable = canUse;

            var canDiscard = CanInput && StageCards.SelectCount > 0 && !DraggedCard;
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
