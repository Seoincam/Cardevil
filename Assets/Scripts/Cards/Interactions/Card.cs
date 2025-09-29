using Cardevil.Cards.Evaluations;
using Cardevil.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.Interactions 
{
    public class Card : MonoBehaviour, IEvaluateVisual, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("Card")]
        public CardData data;
        private bool isDiscarded = false;
        private StageCardsContext ctx;


        [Header("Visual")]
        [SerializeField] GameObject cardVisualPrefab;
        [SerializeField] CardVisualSetting visualSetting;
        [SerializeField] CardVisual _cardVisual;
        private bool _isReroll;

        private Vector3 pointerOffset;
        private float pointerDownTime;
        private float pointerUpTime;


        [Header("Drag")]
        public bool isSelected;
        public bool isDragging;


        [Header("Events")]
        public Action<Card> OnPointerDownEvent;
        public Action<Card> OnPointerUpEvent;
        public Action<Card> OnBeginDragEvent;
        public Action<Card> OnEndDragEvent;
        public Action<Card> OnSelectValueStartEvent;
        public Action<Card> OnSelectValueEndEvent;

        public Action OnDraw;
        public Action OnDiscard;
        public Action OnRerollDraw;
        public Action<Transform> OnRerollDiscard;
        public Action OnDestory;


        private CardHandBar handBar;

        public CardVisual CardVisual
        {
            get => _cardVisual ??= CreateCardVisual();
        }

        public bool IsReroll => _isReroll;

        public int HandIndex => ctx.Hand.IndexOf(this);

        public float NormalizedPosition => Util.Remap(HandIndex, 0, transform.parent.parent.childCount - 1, 0, 1);

        private bool CanDrag
        {
            get
            {
                if (isDiscarded)
                    return false;

                if (_isReroll)
                    return false;

                if (handBar.DraggedCard != null
                    && handBar.DraggedCard != this)
                    return false;

                if (!handBar.CanInput)
                    return false;

                return true;
            }
        }


        public void Init(CardData data, StageCardsContext ctx)
        {
            this.ctx = ctx;
            this.data = data;
            _isReroll = true;
            CardVisual.Init(this);
        }

        public void Init(StageCardsContext ctx, CardHandBar handBar, CardData data)
        {
            this.ctx = ctx;
            this.data = data;
            this.handBar = handBar;
            _isReroll = false;
            CardVisual.Init(this);
        }

        public void SetHandBar(CardHandBar handBar)
        {
            this.handBar = handBar;
            _isReroll = false;
        }




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
                var velocity = direction * Mathf.Min(visualSetting.MoveSpeedLimit, neededVelocity);

                transform.Translate(velocity * Time.deltaTime);
            }
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

                if (!data.CanOpenSelection)
                    return;

                // TODO: 실행자 수정
                handBar.selectContainer.OpenSelection(this);
                OnSelectValueStartEvent?.Invoke(this);
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
                if (pointerUpTime - pointerDownTime > visualSetting.ClickDetectThreshold)
                    return;

                isSelected = Managers.Card.StageCardsCtx.SelectCount < 4 && !isSelected;

                if (isSelected)
                {
                    transform.localPosition = new Vector3(0, visualSetting.SelectOffset, transform.localPosition.z);
                    handBar.Select(this);
                }
                else
                {
                    transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                    handBar.Deselect(this);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnPointerUpEvent?.Invoke(this);
            }

        }



        public void SetSlot(Transform slot, bool isDragging = false)
        {
            transform.SetParent(slot);

            if (!isDragging)
                transform.localPosition = isSelected
                    ? new Vector3(0, visualSetting.SelectOffset, 0)
                    : Vector3.zero;

            _cardVisual.UpdateIndex();
        }

        public void Discard()
        {
            isDiscarded = true;
            OnDiscard?.Invoke();
        }

        public void Destroy()
        {
            OnDestory?.Invoke();
            Destroy(gameObject);
        }

        public void SetReroll(bool isReroll)
        {
            _isReroll = isReroll;
        }

        public void ExecuteEvaluationAction()
        {
            _cardVisual.ExecuteEvaluationAction();
        }

        private CardVisual CreateCardVisual()
        {
            var visualHandler = FindAnyObjectByType<CardVisualHandler>();
            if (visualHandler == null)
                Debug.LogError("Visual Handler를 찾을 수 없음.");

            _cardVisual = Instantiate(original: cardVisualPrefab, parent: visualHandler.transform).GetComponent<CardVisual>();
            return _cardVisual;
        }
    }
}