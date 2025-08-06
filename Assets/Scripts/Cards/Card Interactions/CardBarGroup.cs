using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardBarGroup : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public Card draggedCard { get; private set; }
        private List<Card> cards = new();

        public HashSet<Card> selectedCards = new(4);
        public event Action onSelectedCardsCountChanged;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots = new Transform[6];

        [Header("References")]
        private CardManager cardManager;

        [Header("Select Setting")]
        [SerializeField] float selectOffset = 35f;

        [Header("Etc")]
        public bool CanInteraction => canInteraction && !isSwapping;
        private bool canInteraction = true;
        private bool isSwapping = false;

        public void Init(CardManager cardManager, Action onSelectedCardsCountChanged)
        {
            this.cardManager = cardManager;
            this.onSelectedCardsCountChanged += onSelectedCardsCountChanged;

            _ = InitCard();

            Debug.Log($"덱에 남은 카드: {cardManager.cardDatas.Count}장");

            onSelectedCardsCountChanged?.Invoke();
        }

        void Awake()
        {
            for (int i = 0; i < 6; i++)
            {
                var slot = Instantiate(original: cardSlotPrefab, parent: transform);
                slot.gameObject.SetActive(false);
                slots[i] = slot.transform;
            }
        }

        void Update()
        {
            // 카드 버리기
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                _ = DiscardSequentially();
            }

            if (!canInteraction)
                return;

            if (draggedCard == null)
                return;

            if (isSwapping)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                if (draggedCard.transform.position.x > cards[i].transform.position.x)
                    if (draggedCard.GetSlotIndex() < cards[i].GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }

                if (draggedCard.transform.position.x < cards[i].transform.position.x)
                    if (draggedCard.GetSlotIndex() > cards[i].GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }
            }
        }

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

        public void AddSelectedCard(Card card)
        {
            selectedCards.Add(card);

            var canUseCard = selectedCards.Count > 0;
            onSelectedCardsCountChanged?.Invoke();
        }

        public void RemoveSelectedCard(Card card)
        {
            selectedCards.Remove(card);

            var canUseCard = selectedCards.Count > 0;
            onSelectedCardsCountChanged?.Invoke();
        }


        private void Swap(int index)
        {
            isSwapping = true;

            var selectedCardSlot = draggedCard.transform.parent;
            var swappedSlot = cards[index].transform.parent;

            cards[index].transform.SetParent(selectedCardSlot);
            cards[index].transform.localPosition = cards[index].isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero;

            draggedCard.transform.SetParent(swappedSlot);

            isSwapping = false;
        }

        public async UniTaskVoid InitCard()
        {
            canInteraction = false;

            for (int i = 5; i >= 0; i--)
            {
                slots[i].gameObject.SetActive(true);
                var card = SpawnCard(slotIndex: i);
                await UniTask.Delay(TimeSpan.FromSeconds(.15f));
            }

            canInteraction = true;
        }

        public Card SpawnCard(int slotIndex)
        {
            if (cardManager.cardDatas.Count == 0)
            {
                Debug.LogError("Card Data가 없음.");
                return null;
            }
                    
            var cardData = cardManager.cardDatas.First();
            cardManager.cardDatas.RemoveAt(0);

            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            cards.Add(card);

            // 이벤트 구독
            card.OnBeginDragEvent += BeginDrag;
            card.OnEndDragEvent += EndDrag;

            return card;
        }

        public async UniTaskVoid DiscardSequentially()
        {
            canInteraction = false;
            var duration = .35f;

            // 카드 버리기
            // - - - - - -
            var sortedSelect = selectedCards.OrderBy(c => c.GetSlotIndex())
                .ToList();

            foreach (var card in sortedSelect)
            {
                var slotIndex = card.GetSlotIndex();
                cards.Remove(card);
                card.Discard(duration);
                slots[slotIndex].gameObject.SetActive(false);
                await UniTask.Delay(TimeSpan.FromSeconds(duration));
                card.Destroy();
            }

            selectedCards.Clear();

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
                await UniTask.Delay(TimeSpan.FromSeconds(duration - .1f));
            }

            Debug.Log($"덱에 남은 카드: {cardManager.cardDatas.Count}장");
            canInteraction = true;
        }
        
    }
}
