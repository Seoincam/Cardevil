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
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        
        [Header("State")]
        [SerializeField, VisibleOnly] private int handIndex;
        [SerializeField, VisibleOnly] private float pointerDownTime;
        [SerializeField, VisibleOnly] private float pointerUpTime;
        
        [Flags]
        private enum State
        {
            None = 0,
            
            Selected = 1 << 0,
            Dragging = 1 << 1,
            Discarded = 1 << 2,
            Reroll = 1 << 3,
            AnyCardDragged = 1 << 4, // HandBar에서 드래그되고 있는 카드가 있나? 
        }
        private State _state;
        
        // References
        private Poolable _poolable;
        private Image _image;
        
        public event Action PointerEntered, PointerExited, DragEnd;
        public event Action<Card, CardPointerArgs> PointerDown, PointerUp;
        public event Action<Card> DragStart;
        
        public CanvasGroup VisualCanvasGroup => visual.CanvasGroup;
        public IEvaluateVisual EvaluateVisual => visual;
        public CardData Data => data;

        public bool IsDragging => Is(State.Dragging);
        public bool IsReroll => Is(State.Reroll);
        public int HandIndex => handIndex;
        
        private bool CanInteraction
        {
            get
            {
                if (Is(State.Discarded)) return false;
                if (Is(State.Reroll)) return false;
                if (Is(State.AnyCardDragged) && !Is(State.Dragging)) return false;
                
                return true;
            }
        }
        
        
        private void Awake()
        {
            Clear();
            _poolable = GetComponent<Poolable>();
            _poolable.OnRelease += Clear;
        }
        
        private void Update()
        {
            if (Is(State.Discarded)) return;

            if (Is(State.Dragging))
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
        /// 카드 데이터 및 비주얼 초기화.
        /// 지정된 <see cref="CardData"/>와 <see cref="CardVisualSpriteSet"/>을 기반으로
        /// 카드 오브젝트 생성 및 비주얼 요소 설정.
        /// </summary>
        /// <param name="cardData">카드 데이터 객체</param>
        /// <param name="model">스테이지 카드 모델 참조용 읽기 전용 모델</param>
        public void Init(CardData cardData, IReadOnlyCardsModel model)
        {
            data = cardData;
            _state = State.None;
            
            _image = GetComponent<Image>();
            
            // Card Visual
            var visualHandler = GameObject.Find("Card Visual Transform");
            if (!visualHandler) { LogEx.LogError("Visual Handler를 찾을 수 없음."); return; }

            var go = AssetUtil.Instantiate("Cards/CardVisual", visualHandler.transform);
            visual = go.GetComponent<CardVisual>();
            visual.Init(this, model);
            
            WireVisual(visual);
        }

        public void Clear()
        {
            _state = State.None;
            visual = null;
        }
        
        public async UniTask UpdateVisual()
        {
            await visual.UpdateVisual(Data);
        } 
        
        #region Reroll

        /// <summary>
        /// 리롤 상태 설정.
        /// 지정된 값에 따라 리롤 상태를 갱신하고, 해제 시 비주얼 종료 처리.
        /// </summary>
        /// <param name="value">리롤 상태 값</param>
        public void SetRerollState(bool value)
        {
            Set(State.Reroll, value);
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
            Set(State.Reroll, true);
            visual.AnimateRerollDiscard();
            AssetUtil.Destroy(gameObject);
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
            Set(State.Discarded, true);
            
            UnwireVisual(visual);
            visual.Discard();
            
            AssetUtil.Destroy(gameObject);
        }

        #endregion

        #region Presenter

        /// <summary>
        /// 카드 위치 갱신.
        /// 선택 및 드래그 상태에 따라 로컬 위치 조정.
        /// </summary>
        public void UpdatePosition()
        {
            if (Is(State.Dragging)) return;

            float newY = Is(State.Selected) ? visualSetting.SelectOffset : 0f;
            transform.localPosition = new Vector3(0f, newY, 0f);
        }
        
        /// <summary>
        /// Select 여부를 설정.
        /// </summary>
        public void SetSelect(bool value)
        {
            Set(State.Selected, value);
            float newY = value ? visualSetting.SelectOffset : 0f;
            transform.localPosition = new Vector3(0f, newY, 0f);
        }
        
        /// <summary>
        /// 전체 카드 드래그 상태 설정.
        /// 다른 카드의 드래그 여부를 표시하는 상태 값 지정.
        /// </summary>
        public void SetAnyCardDragged(bool value)
        {
            Set(State.AnyCardDragged, value);
            _image.raycastTarget = !value;
            PointerExited?.Invoke();
        }


        /// <summary>
        /// <see cref="CardVisual"/>의 시각적 index를 업데이트.
        /// </summary>
        public void UpdateVisualIndex(int index)
        {
            handIndex = index;
            visual?.UpdateVisualIndex(index);
        }
        
        #endregion
        
        public void ExecuteEvaluationAction()
        {
            visual.ExecuteEvaluationAction();
        }
        
        // Wire CardVisual
        private void WireVisual(CardVisual cardVisual)
        {
            if (!cardVisual) return;
            
            DragStart += cardVisual.OnDragStart;
            DragEnd += cardVisual.OnDragEnd;

            PointerDown += cardVisual.OnPointerDown;
            PointerUp += cardVisual.OnPointerUp;
            
            PointerEntered += cardVisual.OnPointerEnter;
            PointerExited += cardVisual.OnPointerExit;
        }
        private void UnwireVisual(CardVisual cardVisual)
        {
            if (!cardVisual) return;
            
            DragStart -= cardVisual.OnDragStart;
            DragEnd -= cardVisual.OnDragEnd;
            
            PointerDown -= cardVisual.OnPointerDown;
            PointerUp -= cardVisual.OnPointerUp;
            
            PointerEntered -= cardVisual.OnPointerEnter;
            PointerExited -= cardVisual.OnPointerExit;
        }
        

        #region Pointer Event Interfaces

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteraction)
                return;
            
            PointerEntered?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!CanInteraction)
                return;
            
            PointerExited?.Invoke();
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

            DragStart?.Invoke(this);
            _image.raycastTarget = false;
            Set(State.Dragging, true);
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
            _image.raycastTarget = true;
            
            transform.DOLocalMove(
                    endValue: Is(State.Selected)
                        ? new Vector3(0, visualSetting.SelectOffset, 0)
                        : Vector3.zero,
                    duration: visualSetting.EndDragTweenDuration)
                .SetEase(Ease.OutBack);
            Set(State.Dragging, false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanInteraction)
                return;
            
            PointerUp?.Invoke(this, new CardPointerArgs(Time.time));
        }

        #endregion
        
        public void FadeChangeImage(bool active)
        {
            visual.FadeChangeImage(active);
        }
        
        private bool Is(State state)
        {
            return (this._state & state) != 0;
        }

        private void Set(State state, bool value)
        {
            this._state = (this._state &= ~state) | (value ? state : 0);
        }
    }
}