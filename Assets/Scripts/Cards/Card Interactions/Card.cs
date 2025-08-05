using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.CardInteractinos 
{
    public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("Card")]
        public CardData cardData;
        private bool isDestroyed = false;

        [Header("Visual")]
        [SerializeField] private GameObject cardVisualPrefab;
        public CardVisual cardVisual;

        [Header("Reference")]
        private CardBarGroup barGroup;

        [Header("Selection")]
        public bool isSelected;
        public bool isDragging;

        private Vector3 pointerOffset;
        private float pointerDownTime;
        private float pointerUpTime;
        private float moveSpeedLimit = 100f;

        [Header("Events")]
        [HideInInspector] public Action<Card> OnPointerDownEvent;
        [HideInInspector] public Action<Card> OnPointerUpEvent;
        [HideInInspector] public Action<Card> OnBeginDragEvent;
        [HideInInspector] public Action<Card> OnEndDragEvent;
        [HideInInspector] public Action OnDestoryed;

        void Update()
        {
            ClampPosition();

            if (isDragging)
            {
                var targetPosition = Input.mousePosition - pointerOffset;
                var direction = (targetPosition - transform.position).normalized;

                var neededVelocity = Vector2.Distance(transform.position, targetPosition) / Time.deltaTime;
                var velocity = direction * Mathf.Min(10000, neededVelocity);

                transform.Translate(velocity * Time.deltaTime);
            }
        }

        public void Init(CardBarGroup barGroup, CardData cardData)
        {
            this.barGroup = barGroup;
            this.cardData = cardData;

            transform.name = cardData.type == CardType.Move
                ? cardData.direction.ToString()
                : $"{cardData.color} {cardData.value}";

            var visualHandler = FindAnyObjectByType<CardVisualHandler>();
            if (visualHandler == null)
                Debug.LogError("Visual Handler를 찾을 수 없음.");

            cardVisual = Instantiate(original: cardVisualPrefab, parent: visualHandler.transform).GetComponent<CardVisual>();
            cardVisual.Init(parentCard: this, cardData);
        }

        private void ClampPosition()
        {
            // Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
            // Vector3 clampedPosition = transform.position;
            // clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
            // clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
            // transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;

            OnBeginDragEvent?.Invoke(this);
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;

            OnEndDragEvent?.Invoke(this);
            isDragging = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;

            if (isDragging)
                return;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isDestroyed)
                return; 

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;

            OnPointerDownEvent?.Invoke(this);
            pointerDownTime = Time.time;

            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pointerOffset = transform.position - mousePosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isDestroyed)
                return; 
                
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (barGroup.draggedCard != null
                && barGroup.draggedCard != this)
                return;

            OnPointerUpEvent?.Invoke(this);
            pointerUpTime = Time.time;
            if (pointerUpTime - pointerDownTime > 0.2f)
                return;

            isSelected = barGroup.selectedCards.Count >= 4 
                ? false
                : !isSelected;

            if (isSelected)
            {
                transform.localPosition = new Vector3(0, 35, transform.localPosition.z);
                barGroup.AddSelectedCard(this);
            }
            else
            {
                transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                barGroup.RemoveSelectedCard(this);
            }
        }

        private void DestoryCard()
        {
            isDestroyed = true;
        }



        public int GetSlotIndex()
        {
            return transform.parent.CompareTag("Slot")
                ? transform.parent.GetSiblingIndex()
                : 0;
        }
    }
}