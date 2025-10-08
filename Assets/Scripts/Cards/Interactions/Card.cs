using Cardevil.Attributes;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Cardevil.Cards.Interactions 
{
    [RequireComponent(typeof(Poolable))]
    public class Card : MonoBehaviour, IEvaluateVisual, IClearable,
                        IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;
        
        [Header("Card")]
        [SerializeField, VisibleOnly] private CardData data;
        [SerializeField, VisibleOnly] private CardVisual visual;
        [SerializeField, VisibleOnly] private DragState drag;
        
        public event Action DragStarted, DragEnded, Discarded, Destroyed;
        public event Action<Card, CardPointerArgs> PointerDown, PointerUp;
        
        // TODO: 외부에서 호출되지 않도록 메서드로 감싸기
        public Action<Card> ValueSelectionStarted, ValueSelectionEnded;

        public Action RerollDrawn;
        public Action<Transform> RerollDiscarded;
        public Action RerollEnded;
        
        public Action Drawn; 
        
        private Poolable _poolable;
        private CardHandBar _handBar;
        
        private CardVisual CardVisual
        {
            get
            {
                if (visual == null) visual = CreateCardVisual();
                return visual;

                CardVisual CreateCardVisual()
                {
                    var visualHandler = FindAnyObjectByType<CardVisualHandler>();
                    if (!visualHandler)
                        LogEx.LogError("Visual Handler를 찾을 수 없음.");

                    var v = Managers.Resource.Instantiate("Cards/CardVisual", visualHandler.transform).GetComponent<CardVisual>();
                    return v;
                }
            }
        }
        
        public CardData Data => data;
        public bool IsSelected => drag.isSelected;
        public bool IsDragging => drag.isDragging;
        public bool IsReroll => drag.isReroll;
        private bool CanDrag
        {
            get
            {
                if (drag.isDiscarded) return false;
                if (drag.isReroll) return false;
                if (!_handBar.CanInput) return false;
                if (_handBar.DraggedCard && _handBar.DraggedCard != this) return false;
                return true;
            }
        }

        private void Awake()
        {
            Clear();
            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
        }

        public void Clear()
        {
            drag.isReroll = false;
            drag.isDragging = false;
            drag.isSelected = false;
            drag.isDiscarded = false;
            if (visual != null) { visual.Clear(); visual = null; }
        }

        private void Update()
        {
            if (drag.isDiscarded) return;

            if (drag.isDragging)
            {
                // var targetPosition = Input.mousePosition - pointerOffset;
                var targetPosition = Input.mousePosition;
                var direction = (targetPosition - transform.position).normalized;

                var neededVelocity = Vector2.Distance(transform.position, targetPosition) / Time.deltaTime;
                var velocity = direction * Mathf.Min(visualSetting.MoveSpeedLimit, neededVelocity);

                transform.Translate(velocity * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// HandBar에서 Spawn 후 Initializer.
        /// </summary>
        public void SpawnInHand(CardHandBar handBar, CardData data)
        {
            this.data = data;
            _handBar = handBar;
            drag.isReroll = false;
            CardVisual.Init(this);
        }
        
        /// <summary>
        /// Reroll 중 Spawn 후 Initializer.
        /// </summary>
        public void SpawnAsReroll(CardData data)
        {
            this.data = data;
            drag.isReroll = true;
            CardVisual.Init(this);
        }
        
        /// <summary>
        /// Reroll이 끝나고 HandBar로 옮겨질 때 추가적인 Initializer.
        /// </summary>
        /// <param name="handBar"></param>
        public void CompleteReroll(CardHandBar handBar)
        {
            _handBar = handBar;
            drag.isReroll = false;
            RerollEnded?.Invoke();
        }
        
        /// <summary>
        /// 부모 Slot을 설정. 위치를 결정하는데 사용.
        /// </summary>
        public void SetSlot(Transform slot, bool isDragging = false)
        {
            transform.SetParent(slot);

            if (!isDragging)
                transform.localPosition = drag.isSelected
                    ? new Vector3(0, visualSetting.SelectOffset, 0)
                    : Vector3.zero;

            visual.UpdateIndex();
        }

        public void Discard()
        {
            drag.isDiscarded = true;
            Discarded?.Invoke();
        }

        public void Destroy()
        {
            Destroyed?.Invoke();
            Managers.Resource.Destroy(gameObject);
        }

        public void ExecuteEvaluationAction()
        {
            visual.ExecuteEvaluationAction();
        }
        
        /// <summary>
        /// Select 여부를 결정.
        /// </summary>
        public void SetSelect(bool isSelected)
        {
            drag.isSelected = isSelected;
            var newY = isSelected ? visualSetting.SelectOffset : 0;
            transform.localPosition = new Vector3(0, newY, transform.localPosition.z);
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

                PointerDown?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.LeftMouse));
            }
            
            // TODO: 나중에 그래픽 나오면 우클릭 말고 전환 그래픽 클릭으로 변경하기
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                PointerDown?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.RightMouse));
            
                if (!data.CanOpenSelection)
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

            DragStarted?.Invoke();
            drag.isDragging = true;
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

            DragEnded?.Invoke();
            drag.isDragging = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanDrag)
                return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
                PointerUp?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.LeftMouse));
            // TODO: 나중에 그래픽 나오면 우클릭 말고 전환 그래픽 클릭으로 변경하기
            else if (eventData.button == PointerEventData.InputButton.Right)
                PointerUp?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.RightMouse));
        }

        #endregion

        #region Nested
        
        [Serializable]
        private struct DragState
        {
            public bool isSelected, isDragging, isDiscarded, isReroll;
            public float pointerDownTime, pointerUpTime;
        }

        #endregion
    }
}