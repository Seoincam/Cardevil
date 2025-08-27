using System.Linq;
using UnityEngine;
using DG.Tweening;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using Cardevil.Events;

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

        public void Init()
        {
            Hand = new();

            for (int i = 0; i < 6; i++)
            {
                var slot = Instantiate(original: cardSlotPrefab, parent: transform);
                slot.gameObject.SetActive(false);
                slots[i] = slot.transform;
            }

            useCardButton.onClick.AddListener(TryUseCard);
            discardCardButton.onClick.AddListener(DiscardCard);

            Managers.Turn.PreGameAsync += InitBarGroup;
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
            UpdateCanUseCard();
        }

        public void RemoveSelectedCard(Card card)
        {
            Hand.Deselect(card);
            UpdateCanUseCard();
        }

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
            card.OnSelectEndEvent += UpdateCanUseCard;

            Managers.Card.UpdateDeckCardCount();

            for (int i = 0; i < Hand.HandCount; i++)
            {
                var c = Hand.GetCard(i);
                c.cardVisual.UpdateIndex(c.GetSlotIndex());
            }
                
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

            canInteraction = true;
            UpdateCanUseCard();
        }

        private void TryUseCard()
        {
            Managers.Card.UseCard(Hand.Selects);
            _ = DiscardSequentially();
        }

        private void DiscardCard()
        {
            // TODO: 카드 선택 0개일땐 불가능하게 수정
            useCardButton.interactable = false;
            _ = DiscardSequentially();
        }
    }
}
