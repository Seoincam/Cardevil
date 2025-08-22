using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardBarGroup : MonoBehaviour
    {
        public InGameHand Hand { get; private set; }

        // TODO: SO를 어디서 보관할지 조금 더 고민
        [Header("Card Data")]
        public BaseDeckConfiguration baseDeckConfig;
        public BaseDeckConfiguration baseRuntimeDeckConfig;

        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public Card draggedCard { get; private set; }
        // private List<Card> cards = new();

        // public HashSet<Card> selectedCards = new(4);
        public event Action onSelectedCardsCountChanged;

        public SelectContainer selectContainer;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots = new Transform[6];

        [Header("References")]
        [SerializeField] Button useCardButton;
        [SerializeField] Button discardCardButton;

        [Header("Setting")]
        [SerializeField] float selectOffset = 35f;
        [SerializeField] float drawInterval = .2f;
        [SerializeField] float discardInterval = .3f;

        [Header("Etc")]
        public bool CanInteraction => canInteraction && !isSwapping;
        private bool canInteraction = true;
        private bool isSwapping = false;

        public void Init(Action onSelectedCardsCountChanged)
        {
            Hand = new();

            for (int i = 0; i < 6; i++)
            {
                var slot = Instantiate(original: cardSlotPrefab, parent: transform);
                slot.gameObject.SetActive(false);
                slots[i] = slot.transform;
            }

            this.onSelectedCardsCountChanged += onSelectedCardsCountChanged;
            useCardButton.onClick.AddListener(TryUseCard);
            discardCardButton.onClick.AddListener(DiscardCard);

            Managers.Turn.PreGameAsync += InitBarGroup;
        }

        void Update()
        {
            if (!CanInteraction)
                return;
            if (draggedCard == null)
                return;
            DetectSwap();
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

        public void AddSelectedCard(Card card)
        {
            Hand.Select(card);
            onSelectedCardsCountChanged?.Invoke();
        }

        public void RemoveSelectedCard(Card card)
        {
            Hand.Deselect(card);
            onSelectedCardsCountChanged?.Invoke();
        }

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

            var selectedCardSlot = draggedCard.transform.parent;
            var swappedSlot = Hand.GetCard(index).transform.parent;

            Hand.GetCard(index).transform.SetParent(selectedCardSlot);
            Hand.GetCard(index).transform.localPosition = Hand.GetCard(index).isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero;

            draggedCard.transform.SetParent(swappedSlot);

            isSwapping = false;
        }

        public async UniTask InitBarGroup()
        {
            canInteraction = false;

            for (int i = 5; i >= 0; i--)
            {
                slots[i].gameObject.SetActive(true);
                var card = SpawnCard(slotIndex: i);
                await UniTask.Delay(TimeSpan.FromSeconds(.15f));
            }

            canInteraction = true;
            onSelectedCardsCountChanged?.Invoke();
        }

        public Card SpawnCard(int slotIndex)
        {
            var cardData = Managers.Card.Deck.DrawCard();
            if (cardData == null)
                return null;

            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            Hand.Draw(card);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;

            Managers.Card.UpdateDeckCardCount();
            return card;
        }

        public async UniTaskVoid DiscardSequentially()
        {
            canInteraction = false;

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

            // 다시 뽑기
            // - - - - - -
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));

            foreach (var slot in inactiveSlots)
            {
                var slotIndex = slot.GetSiblingIndex();
                slot.gameObject.SetActive(true);
                SpawnCard(slotIndex);
                await UniTask.Delay(TimeSpan.FromSeconds(drawInterval));
            }

            onSelectedCardsCountChanged?.Invoke();
            canInteraction = true;
        }

        private void TryUseCard()
        {
            if (!Managers.Card.CanUseCard)
                return;

            Managers.Card.UseCard(Hand.Selects);

            _ = DiscardSequentially();
        }

        private void DiscardCard()
        {
            // TODO: 카드 선택 0개일땐 불가능하게 수정
            useCardButton.interactable = false;
            _ = DiscardSequentially();
        }

        public void SetUseCardButton(bool interactable)
        {
            useCardButton.interactable = interactable;
        }
    }
}
