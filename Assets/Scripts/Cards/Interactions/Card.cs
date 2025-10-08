using Cardevil.Attributes;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.Interactions 
{
    [RequireComponent(typeof(Poolable))]
    public class Card : MonoBehaviour, IEvaluateVisual, IClearable,
                        IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("SO")]
        [SerializeField] CardVisualSettingSO visualSetting;
        
        [Header("Card")]
        [SerializeField, VisibleOnly] CardData _data;
        [SerializeField, VisibleOnly] CardVisual _visual;
        [SerializeField, VisibleOnly] DragState _drag;

        public event Action<Card> PointerDown, PointerUp;
        public event Action<Card> DragStarted, DragEnded;
        public Action<Card> ValueSelectionStarted, ValueSelectionEnded;

        public Action RerollDrawn;
        public Action<Transform> RerollDiscarded;
        public Action RerollEnded;
        public Action Drawn, Discarded, Destroyed;



        private Poolable _poolable;
        private CardHandBar _handBar;



        public CardData Data => _data;

        public CardVisual CardVisual
        {
            get
            {
                if (_visual == null) _visual = CreateCardVisual();
                return _visual;
            }
        }

        public bool IsSelected  => _drag.isSelected;
        public bool IsDragging  => _drag.isDragging;
        public bool IsReroll    => _drag.isReroll;

        private bool CanDrag
        {
            get
            {
                if (_drag.isDiscarded) return false;
                if (_drag.isReroll) return false;
                if (!_handBar.CanInput) return false;
                if (_handBar.DraggedCard != null && _handBar.DraggedCard != this) return false;

                return true;
            }
        }



        void Awake()
        {
            Clear();
            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
        }

        public void Clear()
        {
            _drag.isReroll = false;
            _drag.isDragging = false;
            _drag.isSelected = false;
            _drag.isDiscarded = false;
            _visual = null;
        }


        public void SpawnInHand(CardHandBar handBar, CardData data)
        {
            _data = data;
            _handBar = handBar;
            _drag.isReroll = false;
            CardVisual.Init(this);
        }

        public void SpawnAsReroll(CardData data)
        {
            _data = data;
            _drag.isReroll = true;
            CardVisual.Init(this);
        }

        public void CompleteReroll(CardHandBar handBar)
        {
            _handBar = handBar;
            _drag.isReroll = false;
            RerollEnded?.Invoke();
        }

        void Update()
        {
            if (_drag.isDiscarded) return;

            if (_drag.isDragging)
            {
                // var targetPosition = Input.mousePosition - pointerOffset;
                var targetPosition = Input.mousePosition;
                var direction = (targetPosition - transform.position).normalized;

                var neededVelocity = Vector2.Distance(transform.position, targetPosition) / Time.deltaTime;
                var velocity = direction * Mathf.Min(visualSetting.MoveSpeedLimit, neededVelocity);

                transform.Translate(velocity * Time.deltaTime);
            }
        }

        public void SetSlot(Transform slot, bool isDragging = false)
        {
            transform.SetParent(slot);

            if (!isDragging)
                transform.localPosition = _drag.isSelected
                    ? new Vector3(0, visualSetting.SelectOffset, 0)
                    : Vector3.zero;

            _visual.UpdateIndex();
        }

        public void Discard()
        {
            _drag.isDiscarded = true;
            Discarded?.Invoke();
        }

        public void Destroy()
        {
            Destroyed?.Invoke();
            Managers.Resource.Destroy(gameObject);
        }

        public void ExecuteEvaluationAction()
        {
            _visual.ExecuteEvaluationAction();
        }

        private CardVisual CreateCardVisual()
        {
            var visualHandler = FindAnyObjectByType<CardVisualHandler>();
            if (visualHandler == null)
                LogEx.LogError("Visual Handler를 찾을 수 없음.");

            var v = Managers.Resource.Instantiate("Cards/CardVisual", visualHandler.transform).GetComponent<CardVisual>();
            return v;
        }
        


        #region Point Event

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

                PointerDown?.Invoke(this);
                _drag.pointerDownTime = Time.time;

                var mousePosition = Input.mousePosition;
                var offset = mousePosition - transform.position;
            }

            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                PointerDown?.Invoke(this);

                if (!_data.CanOpenSelection)
                    return;

                // TODO: 실행자 수정
                _handBar.selectContainer.OpenSelection(this);
                ValueSelectionStarted?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!CanDrag)
                return;

            DragStarted?.Invoke(this);
            _drag.isDragging = true;
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

            DragEnded?.Invoke(this);
            _drag.isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!CanDrag)
                    return;

                PointerUp?.Invoke(this);
                _drag.pointerUpTime = Time.time;
                if (_drag.pointerUpTime - _drag.pointerDownTime > visualSetting.ClickDetectThreshold)
                    return;

                _drag.isSelected = Managers.Card.StageCardsCtx.SelectCount < 4 && !_drag.isSelected;

                if (_drag.isSelected)
                {
                    transform.localPosition = new Vector3(0, visualSetting.SelectOffset, transform.localPosition.z);
                    _handBar.Select(this);
                }
                else
                {
                    transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                    _handBar.Deselect(this);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                PointerUp?.Invoke(this);
            }
        }

        #endregion



        [Serializable]
        private struct DragState
        {
            public bool isSelected, isDragging, isDiscarded, isReroll;
            public float pointerDownTime, pointerUpTime;
        }
    }
}