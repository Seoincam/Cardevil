using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Events;
using Cardevil.Systems;

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
        public bool CanInteraction => canInteraction && !isSwapping;
        private bool canInteraction = true; // 카드가 사용 가능한가?
        private bool isSwapping = false; // 카드가 정렬 등 움직이고 있나?



        void Update()
        {
            if (!CanInteraction)
                return;
            if (draggedCard == null)
                return;
            DetectSwap();
        }



        public void Init()
        {
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

            useCardButton.onClick.AddListener(UseCard);
            discardCardButton.onClick.AddListener(DiscardCard);

            Managers.Event.GameStateChangeEvent.AddListener(OnGameStateChanged);
        }

        public async UniTask InitBarGroup()
        {
            canInteraction = false;
            UpdateCanUseCard();

            for (int i = 5; i >= 0; i--)
            {
                slots[i].gameObject.SetActive(true);
                var card = SpawnCard(slotIndex: i);
                await UniTask.Delay(TimeSpan.FromSeconds(.15f));
            }

            canInteraction = true;
            UpdateCanUseCard();
        }

        public void AddSelectedCard(Card card)
        {
            Hand.Select(card);
            UpdateCanUseCard();
        }

        public void RemoveSelectedCard(Card card)
        {
            Hand.Deselect(card);
            UpdateCanUseCard();
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
            isSwapping = true;
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
            isSwapping = false;
        }

        #endregion


        #region Spawn & Use card
        
        public Card SpawnCard(int slotIndex)
        {
            var cardData = Deck.DrawCard();
            if (cardData == null)
                return null;

            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            Hand.Draw(card);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;
            card.OnSelectEndEvent += UpdateCanUseCard;

            UpdateDeckCardCount();

            for (int i = 0; i < Hand.HandCount; i++)
            {
                var c = Hand.GetCard(i);
                c.cardVisual.UpdateIndex(c.GetSlotIndex());
            }

            return card;
        }

        private void UseCard()
        {
            CardResultEvaluator.Evaluate(Context, Hand.Selects);
            _ = DiscardSequentially();
            // TODO: HandleUserInput await 끝내기
        }

        private void DiscardCard()
        {
            // TODO: 카드 선택 0개일땐 불가능하게 수정
            useCardButton.interactable = false;
            _ = DiscardSequentially();
        }

        public async UniTaskVoid DiscardSequentially()
        {
            isSwapping = true;

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


            isSwapping = false;
            UpdateCanUseCard();
        }

        #endregion

        private void OnGameStateChanged(GameStateChangeArgs args)
        {
            // 이럴거면 args가 필요할까?
            UpdateCanUseCard();
        }

        // 플레이어 턴이면서 카드 값 선택이 바뀔 때, -> Card.onselectEnded에서 호출
        public void UpdateCanUseCard(Card _)
        {
            UpdateCanUseCard();
        }

        private void UpdateCanUseCard()
        {
            var canUseCard = Managers.Game.currentState != GameManager.GameState.PlayerInput
                ? false
                : CanInteraction && Hand.SelectCount > 0 && Hand.AllValueSelected;
            useCardButton.interactable = canUseCard;

        }

        public void UpdateDeckCardCount()
        {
            // using (var args = RemainingCardChangeArgs.Get())
            // {
            //     args.Init(Deck.Count);
            //     Managers.Event.RemainingCardChangeEvent?.Invoke(args);
            // }
        }


        #region IUserInput
        public bool IsNoCard => false;

        public void ActivateInteraction()
        {
            canInteraction = true;
        }

        public void InactivateInteraction()
        {
            canInteraction = false;
        }

        public async UniTask DrawCard()
        {
            isSwapping = true;

            var inactiveSlots = slots.Where(s => !s.gameObject.activeSelf)
                        .ToArray();

            foreach (var slot in inactiveSlots)
            {
                var slotIndex = slot.GetSiblingIndex();
                slot.gameObject.SetActive(true);
                SpawnCard(slotIndex);
                await UniTask.Delay(TimeSpan.FromSeconds(drawInterval));
            }

            isSwapping = false;
        }

        public async UniTask WaitUserInput()
        {
            // TODO: 기다림 로직
        }
        #endregion
    }
}
