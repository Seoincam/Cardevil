using Cardevil.Attributes;
using Cardevil.Cards.Evaluations;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using DG.Tweening;
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
        [SerializeField, VisibleOnly] private CardState state;
        
        public event Action DragStarted, DragEnded, Discarded, Destroyed;
        public event Action<Card, CardPointerArgs> PointerDown, PointerUp;
        public Action<Card> ValueSelectionStarted, ValueSelectionEnded; // TODO: 외부에서 호출되지 않도록 메서드로 감싸기

        public Action RerollDrawn, RerollEnded;
        public Action<Transform> RerollDiscarded;
        public Action Drawn; 
        
        private Poolable _poolable;
        
        public CardData Data => data;
        public bool IsDragging => state.isDragging;
        public bool IsReroll => state.isReroll;
        
        private bool CanDrag
        {
            get
            {
                if (state.isDiscarded) return false;
                if (state.isReroll) return false;
                if (state.isAnyCardDragged && !state.isDragging) return false;
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
            state = new CardState();
            if (visual) { visual.Clear(); visual = null; }
        }

        private void Update()
        {
            if (state.isDiscarded) return;

            if (state.isDragging)
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
        /// 주어진 데이터를 바탕으로 초기화.
        /// Visual도 함께 생성.
        /// </summary>
        public void Init(CardData data)
        {
            this.data = data;
            
            // Card Visual
            var visualHandler = GameObject.Find("Card Visual Transform");
            if (!visualHandler) { LogEx.LogError("Visual Handler를 찾을 수 없음."); return; }

            var go = Managers.Resource.Instantiate("Cards/CardVisual", visualHandler.transform);
            visual = go.GetComponent<CardVisual>();
            visual.Init(this);
        }

        public void SetRerollState(bool value)
        {
            state.isReroll = value;
            if (!value) RerollEnded?.Invoke();
        }
        
        /// <summary>
        /// Slot 상의 Index를 업데이트함.
        /// </summary>
        public void UpdateIndex(int slotIndex)
        {
            if (!state.isDragging)
            {
                float newY = state.isSelected ? visualSetting.SelectOffset : 0;
                transform.localPosition = new Vector3(0, newY, 0);
            }

            visual.UpdateIndex(slotIndex);
        }

        public void Discard()
        {
            state.isDiscarded = true;
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
        /// Select 여부를 설정.
        /// </summary>
        public void SetSelect(bool value)
        {
            state.isSelected = value;
            float newY = value ? visualSetting.SelectOffset : 0;
            transform.localPosition = new Vector3(0, newY, 0);
        }

        public void SetAnyCardDragged(bool value)
        {
            state.isAnyCardDragged = value;
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
                // _handBar.selectContainer.OpenSelection(this);
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
            state.isDragging = true;
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
            transform.DOLocalMove(
                    endValue: state.isSelected
                        ? new Vector3(0, visualSetting.SelectOffset, 0)
                        : Vector3.zero,
                    duration: visualSetting.EndDragTweenDuration)
                .SetEase(Ease.OutBack);
            state.isDragging = false;
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
        private struct CardState
        {
            public bool isSelected, isDragging, isDiscarded, isReroll;
            public bool isAnyCardDragged; // HandBar에서 드래그되고 있는 카드가 있나?
            public float pointerDownTime, pointerUpTime;
        }

        #endregion
    }
}