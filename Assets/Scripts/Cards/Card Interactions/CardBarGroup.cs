using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace Cardevil.Cards.CardInteractinos
{
    public class CardBarGroup : MonoBehaviour
    {
        [Header("Card")]
        [SerializeField] GameObject cardPrefab;
        public Card selectedCard { get; private set; }
        public List<Card> cards;

        [Header("Slots")]
        [SerializeField] GameObject cardSlotPrefab;
        private Transform[] slots = new Transform[6];

        [Header("Reference")]
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
                _isOnTrashCan = value;
                if (selectedCard == null)
                    return;

                selectedCard.cardVisual.SetCardOnTrashCan(value);
            }
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
            if (selectedCard == null)
                return;

            if (isSwapping)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                if (selectedCard.transform.position.x > cards[i].transform.position.x)
                    if (selectedCard.GetSlotIndex() < cards[i].GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }

                if (selectedCard.transform.position.x < cards[i].transform.position.x)
                    if (selectedCard.GetSlotIndex() > cards[i].GetSlotIndex())
                    {
                        Swap(i);
                        break;
                    }
            }
        }

        public void Init(CardManager cardManager)
        {
            this.cardManager = cardManager;

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
            selectedCard = card;
        }

        private void EndDrag(Card card)
        {
            if (selectedCard == null)
                return;

            selectedCard.transform.DOLocalMove(
                endValue: selectedCard.isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero,
                duration: .2f
            )
            .SetEase(Ease.OutBack);

            selectedCard = null;
        }

        private void Swap(int index)
        {
            isSwapping = true;

            var selectedCardSlot = selectedCard.transform.parent;
            var swappedSlot = cards[index].transform.parent;

            cards[index].transform.SetParent(selectedCardSlot);
            cards[index].transform.localPosition = cards[index].isSelected
                    ? new Vector3(0, selectOffset, 0)
                    : Vector3.zero;

            selectedCard.transform.SetParent(swappedSlot);

            isSwapping = false;
        }
    }
}
