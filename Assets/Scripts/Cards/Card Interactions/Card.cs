using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.CardInteractinos 
{
    public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("Card")]
        public CardData data;
        private bool isDiscarded = false;

        [Header("Visual")]
        [SerializeField] private GameObject cardVisualPrefab;
        public CardVisual cardVisual;

        [Header("Reference")]
        private CardBarGroup barGroup;

        [Header("Drag")]
        public bool isSelected;
        public bool isDragging;
        private bool CanDrag
        {
            get
            {
                if (isDiscarded)
                    return false;

                if (barGroup.draggedCard != null
                    && barGroup.draggedCard != this)
                    return false;

                if (!barGroup.CanInteraction)
                    return false;

                return true;
            }
        }

        private Vector3 pointerOffset;
        private float pointerDownTime;
        private float pointerUpTime;
        private float moveSpeedLimit = 4000;

        [Header("Events")]
        [HideInInspector] public Action<Card> OnPointerDownEvent;
        [HideInInspector] public Action<Card> OnPointerUpEvent;
        [HideInInspector] public Action<Card> OnBeginDragEvent;
        [HideInInspector] public Action<Card> OnEndDragEvent;
        [HideInInspector] public Action<Card> OnSelectStartEvent;
        [HideInInspector] public Action<Card> OnSelectEndEvent;

        [HideInInspector] public Action OnSpawn;
        [HideInInspector] public Action<float> OnDiscard;
        [HideInInspector] public Action OnDestory;

        void Update()
        {
            if (isDiscarded)
                return; 

            // ClampPosition();

            if (isDragging)
            {
                var targetPosition = Input.mousePosition - pointerOffset;
                var direction = (targetPosition - transform.position).normalized;

                var neededVelocity = Vector2.Distance(transform.position, targetPosition) / Time.deltaTime;
                var velocity = direction * Mathf.Min(moveSpeedLimit, neededVelocity);

                transform.Translate(velocity * Time.deltaTime);
            }
        }

        public void Init(CardBarGroup barGroup, CardData cardData)
        {
            this.barGroup = barGroup;
            data = cardData;

            // 이름 할당 (임시)
            if (cardData is DirectionCardData direction)
                transform.name = direction.value.ToString();
            else if (cardData is NumberCardData number)
                transform.name = $"{number.color} {number.value}";
            else
                Debug.LogError("cardData가 어떤 타입도 아닙니다.");

            var visualHandler = FindAnyObjectByType<CardVisualHandler>();
            if (visualHandler == null)
                Debug.LogError("Visual Handler를 찾을 수 없음.");

            cardVisual = Instantiate(original: cardVisualPrefab, parent: visualHandler.transform).GetComponent<CardVisual>();
            cardVisual.Init(this);

            OnSpawn?.Invoke();
        }

        private void ClampPosition()
        {
            // Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
            // Vector3 clampedPosition = transform.position;
            // clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
            // clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
            // transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanDrag)
                return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CanDrag)
                return;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!CanDrag)
                    return;

                OnPointerDownEvent?.Invoke(this);
                pointerDownTime = Time.time;

                var mousePosition = Input.mousePosition;
                var offset = mousePosition - transform.position;
            }

            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnPointerDownEvent?.Invoke(this);

                if (data.OpenSelection(barGroup.selectContainer, this))
                    OnSelectStartEvent?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!CanDrag)
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

            if (!CanDrag)
                return;

            OnEndDragEvent?.Invoke(this);
            isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!CanDrag)
                    return;

                OnPointerUpEvent?.Invoke(this);
                pointerUpTime = Time.time;
                if (pointerUpTime - pointerDownTime > 0.2f)
                    return;

                isSelected = barGroup.Hand.SelectCount >= 4
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
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnPointerUpEvent?.Invoke(this);
            }

        }



        public void Discard(float discardDuration)
        {
            isDiscarded = true;
            OnDiscard?.Invoke(discardDuration);
        }

        public void Destroy()
        {
            OnDestory?.Invoke();
            Destroy(gameObject);
        }

        public int GetSlotIndex()
        {
            return transform.parent.CompareTag("Slot")
                ? transform.parent.GetSiblingIndex()
                : 0;
        }
    }
}