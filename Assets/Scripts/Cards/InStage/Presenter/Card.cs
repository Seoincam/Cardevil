using Cardevil.Attributes;
using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Evaluations;
using Cardevil.Cards.InStage.Model.ReadOnly;
using Cardevil.Cards.InStage.View;
using Cardevil.Cards.ScriptableObjects;
using Cardevil.Cards.Visual.StateMachine;
using Cardevil.Core;
using Cardevil.Pools;
using Cardevil.Utils;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Cardevil.Cards.InStage.Presenter
{
    [RequireComponent(typeof(Poolable))]
    public class Card : MonoBehaviour, IClearable,
                        IDragHandler, IBeginDragHandler, IEndDragHandler, 
                        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;
        
        [Header("Card")]
        [SerializeField, VisibleOnly] private CardData data;
        [SerializeField, VisibleOnly] private CardVisual visual;
        [SerializeField, VisibleOnly] private CardState state;
        
        private event Action DragStart, DragEnd;
        private event Action<Card, CardPointerArgs> PointerDown, PointerUp;
        private event Action<Card> SelectionButtonTapped;
        
        private Poolable _poolable;
        private IReadOnlyStageCardsModel _model;

        public IEvaluateVisual EvaluateVisual => visual;
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

        /// <summary>
        /// 카드 데이터 및 비주얼 초기화.
        /// 지정된 <see cref="CardData"/>와 <see cref="CardVisualSpriteSet"/>을 기반으로
        /// 카드 오브젝트 생성 및 비주얼 요소 설정.
        /// </summary>
        /// <param name="cardData">카드 데이터 객체</param>
        /// <param name="model">스테이지 카드 모델 참조용 읽기 전용 모델</param>
        public void Init(CardData cardData, IReadOnlyStageCardsModel model)
        {
            data = cardData;
            _model = model;
            
            // Card Visual
            var visualHandler = GameObject.Find("Card Visual Transform");
            if (!visualHandler) { LogEx.LogError("Visual Handler를 찾을 수 없음."); return; }

            var go = Managers.Resource.Instantiate("Cards/CardVisual", visualHandler.transform);
            visual = go.GetComponent<CardVisual>();
            visual.Init(this, model);
            if (data.CanOpenSelection)
                visual.BindSelectionButton(OnValueSelectionTapped);
            
            WireVisual(visual);
        }

        public void Clear()
        {
            state = new CardState();
            visual = null;
        }

        public void UpdateVisual() => visual.UpdateVisual();
        
        private void Awake()
        {
            Clear();
            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
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

        private void OnValueSelectionTapped()
        {
            SelectionButtonTapped?.Invoke(this);
        }
        
        #region Reroll

        /// <summary>
        /// 리롤 상태 설정.
        /// 지정된 값에 따라 리롤 상태를 갱신하고, 해제 시 비주얼 종료 처리.
        /// </summary>
        /// <param name="value">리롤 상태 값</param>
        public void SetRerollState(bool value)
        {
            state.isReroll = value;
            if (!value) 
                visual.EndReroll();
        }
        
        /// <summary>
        /// 리롤 드로우 애니메이션 실행.
        /// 비주얼 연출 수행.
        /// </summary>
        public void DoRerollDraw()
        {
            visual.AnimateRerollDraw();
        }
        
        /// <summary>
        /// (리롤) 버리기 처리.
        /// 리롤 상태 설정 후 버리기 애니메이션 실행 및 카드 오브젝트 풀에 반환.
        /// </summary>
        public void DoRerollDiscard()
        {
            state.isReroll = true;
            visual.AnimateRerollDiscard();
            Managers.Resource.Destroy(gameObject);
        }

        #endregion

        #region Draw/Discard

        /// <summary>
        /// 뽑기 애니메이션 실행.
        /// </summary>
        public void DoDraw()
        {
            visual.AnimateDraw();
        }
        
        /// <summary>
        /// 버리기 처리.
        /// 카드 버리기 상태 설정 후 비주얼 해제 및 오브젝트 풀에 반환.
        /// </summary>
        public void DoDiscard()
        {
            state.isDiscarded = true;
            
            UnwireVisual(visual);
            visual.Discard();
            
            Managers.Resource.Destroy(gameObject);
        }

        #endregion

        #region Presenter

        /// <summary>
        /// 카드 위치 갱신.
        /// 선택 및 드래그 상태에 따라 로컬 위치 조정.
        /// </summary>
        public void UpdatePosition()
        {
            if (state.isDragging) return;

            float newY = state.isSelected ? visualSetting.SelectOffset : 0;
            transform.localPosition = new Vector3(0, newY, 0);
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
        
        /// <summary>
        /// 전체 카드 드래그 상태 설정.
        /// 다른 카드의 드래그 여부를 표시하는 상태 값 지정.
        /// </summary>
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
        
        #endregion
        
        public void ExecuteEvaluationAction()
        {
            visual.ExecuteEvaluationAction();
        }
        
        // Wire By StageCardsPresenter
        public void AddDragStart(Action h) => DragStart += h;
        public void RemoveDragStart(Action h) => DragStart -= h;
        
        public void AddDragEnd(Action h) => DragEnd += h;
        public void RemoveDragEnd(Action h) => DragEnd -= h;

        public void AddPointerDown(Action<Card, CardPointerArgs> h) => PointerDown += h;
        public void RemovePointerDown(Action<Card, CardPointerArgs> h) => PointerDown -= h;

        public void AddPointerUp(Action<Card, CardPointerArgs> h) => PointerUp += h;
        public void RemovePointerUp(Action<Card, CardPointerArgs> h) => PointerUp -= h;
        
        public void AddSelectionButtonTapped(Action<Card> h) => SelectionButtonTapped += h;
        public void RemoveSelectionButtonTapped(Action<Card> h) => SelectionButtonTapped -= h;
        
        // Wire CardVisual
        private void WireVisual(CardVisual cardVisual)
        {
            if (!cardVisual) return;
            
            DragStart += cardVisual.OnDragStart;
            DragEnd += cardVisual.OnDragEnd;

            PointerDown += cardVisual.OnPointerDown;
            PointerUp += cardVisual.OnPointerUp;
        }
        private void UnwireVisual(CardVisual cardVisual)
        {
            if (!cardVisual) return;
            
            DragStart -= cardVisual.OnDragStart;
            DragEnd -= cardVisual.OnDragEnd;
            
            PointerDown -= cardVisual.OnPointerDown;
            PointerUp -= cardVisual.OnPointerUp;
        }
        

        #region Pointer Event Interfaces

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

                PointerDown?.Invoke(this, new CardPointerArgs(Time.time));
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!CanInteraction)
                return;

            DragStart?.Invoke();
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

            DragEnd?.Invoke();
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
            
            PointerUp?.Invoke(this, new CardPointerArgs(Time.time));
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