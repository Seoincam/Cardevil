using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardBarGroup : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public Card draggedCard { get; private set; }
        public List<Card> cards;

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
        private bool isSwapping;

        private bool _isOnTrashCan;
        public bool isOnTrashCan
        {
            get => _isOnTrashCan;
            set
            {
                // 임시 로직. 추후 수정
                _isOnTrashCan = value;
                if (draggedCard == null)
                    return;

                draggedCard.cardVisual.SetCardOnTrashCan(value);
            }
        }


        public void Init(CardManager cardManager, Action onSelectedCardsCountChanged)
        {
            this.cardManager = cardManager;
            this.onSelectedCardsCountChanged += onSelectedCardsCountChanged;

            for (int i = 5; i >= 0; i--)
            {
                if (cardManager.cardDatas.Count == 0)
                    Debug.LogError("Card Data가 없음.");

                var cardData = cardManager.cardDatas.First();
                cardManager.cardDatas.RemoveAt(0);

                var card = SpawnCard(slotIndex: i, cardData);

                card.BeginDragEvent += BeginDrag;
                card.EndDragEvent += EndDrag;
            }

            onSelectedCardsCountChanged?.Invoke();
        }

        void Awake()
        {
            for (int i = 0; i < 6; i++)
            {
                var slot = Instantiate(original: cardSlotPrefab, parent: transform);
                slot.transform.name = $"Slot {i}";
                slots[i] = slot.transform;
            }
        }

        void Update()
        {
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

        public Card SpawnCard(int slotIndex, CardData cardData)
        {
            var card = Instantiate(original: cardPrefab, parent: slots[slotIndex]).GetComponent<Card>();
            card.Init(barGroup: this, cardData);

            cards.Add(card);
            return card;
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
    }
}
