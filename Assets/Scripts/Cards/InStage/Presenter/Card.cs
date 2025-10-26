using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Modifiers;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.View;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Cardevil.Cards.InStage.Presenter
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
        
        public event Action DragStarted, DragEnded, Discarded;
        public event Action<Card, CardPointerArgs> PointerDown, PointerUp;
        public Action<Card> ValueSelectionStarted, ValueSelectionEnded; // TODO: 외부에서 호출되지 않도록 메서드로 감싸기

        public Action Drawn; 
        
        private Poolable _poolable;
        private IReadOnlyStageCardsModel _model;
        
        public CardData Data => data;
        public bool IsDragging => state.isDragging;
        public bool IsReroll => state.isReroll;
        
        private bool CanInteraction
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
            visual = null;
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
        public void Init(CardData data, CardVisualSpriteSet visualSpriteSet, IReadOnlyStageCardsModel model)
        {
            this.data = data;
            _model = model;
            
            // Card Visual
            var visualHandler = GameObject.Find("Card Visual Transform");
            if (!visualHandler) { LogEx.LogError("Visual Handler를 찾을 수 없음."); return; }

            var go = Managers.Resource.Instantiate("Cards/CardVisual", visualHandler.transform);
            visual = go.GetComponent<CardVisual>();
            visual.Init(this, visualSpriteSet, model);
        }

        public void SetRerollState(bool value)
        {
            state.isReroll = value;
            // if (!value) RerollEnded?.Invoke();
            if (!value) visual.EndReroll();
        }
        
        
        public void UpdatePosition()
        {
            if (state.isDragging) return;

            float newY = state.isSelected ? visualSetting.SelectOffset : 0;
            transform.localPosition = new Vector3(0, newY, 0);
        }

        public void Discard()
        {
            state.isDiscarded = true;
            Discarded?.Invoke();
            Managers.Resource.Destroy(gameObject);
        }

        public void DoRerollDraw()
        {
            visual.AnimateRerollDraw();
        }
        
        public void DoRerollDiscard()
        {
            state.isReroll = true;
            // RerollDiscarded?.Invoke(target);
            visual.AnimateRerollDiscard();
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

        /// <summary>
        /// <see cref="StageCardsPresenter"/>의
        /// <c>_handChanged</c>를 구독.
        /// <see cref="CardVisual"/>의 시각적 index를 업데이트.
        /// </summary>
        public void OnHandChanged()
        {
            visual?.UpdateVisualIndex();
        }


        #region Point Event

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteraction)
                return;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CanInteraction)
                return;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!CanInteraction)
                    return;

                PointerDown?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.LeftMouse));
            }
            
            // TODO: 나중에 그래픽 나오면 우클릭 말고 전환 그래픽 클릭으로 변경하기
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                PointerDown?.Invoke(this, new CardPointerArgs(Time.time, MouseButton.RightMouse));
                
                // TODO: 값 선택가능한가?
                if (true)
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

            if (!CanInteraction)
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

            if (!CanInteraction)
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
            if (!CanInteraction)
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