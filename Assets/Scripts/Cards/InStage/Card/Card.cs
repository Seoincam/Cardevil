using Cardevil.Attributes;
using Cardevil.Cards.Config;
using Cardevil.Cards.Core;
using Cardevil.Cards.Visual;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
    // TODO:
    // 소환될 때 CardVisual / Changeable ... 분기 처리하기 
    // 일단은 ChangeableCardVisual을 사용함.
    public partial class Card : MonoBehaviour, 
        IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [Header("SO")]
        [SerializeField] private CardVisualSettingSO visualSetting;
        [SerializeField] private CardChangeValueFlipSetting changeFlipSetting;

        [Header("Data")] 
        [field: SerializeField, VisibleOnly] public CardData Data { get; private set; }

        [Header("State")]
        [SerializeField] private State debugState; 
        [SerializeField, VisibleOnly] private Vector3 targetPosition;
        
        [Header("Visual")] 
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private RectTransform visualRoot;
        
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private ChangeableCardVisual visual;

        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private CardFlipComponent flip;

        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private Image changeableMarker;
        
        private State _state;
        
        public static implicit operator CardData(Card card) => card.Data;
        
        /// <summary>
        /// Lerp 이동을 사용할지 여부.
        /// Reroll 중이거나 트윈 중이면 <c>false</c>를 반환함.
        /// </summary>
        private bool UseLerp => !Is(State.Rerolling) && !_isTweening;
        
        /// <summary>
        /// 드래그되고 있지 않을 때 돌아갈 원점.
        /// 선택이 된 상태라면 위로 <c>visualSetting.SelectOffset</c>만큼 더 올라감.
        /// </summary>
        private Vector3 LocalZeroPosition =>
            Is(State.Selected) ? new Vector3(0, visualSetting.SelectOffset) : Vector3.zero; 

        private bool CanInteract
        {
            get
            {
                if (Is(State.Discarded)) return false;
                if (Is(State.Rerolling)) return false;
                if (Is(State.AnyCardDragging) && !Is(State.Dragging)) return false;
                return true;
            }
        }
        
        public void Initialize(CardData cardData)
        {
            Data = cardData;
            _state = State.None;
            debugState = _state;
            
            InitializeVisual(cardData);
        }

        public void Update()
        {
            if (Is(State.Discarded)) return;
            
            UpdateTargetPosition();
            SmoothFollowPosition();
        }

        private void UpdateTargetPosition()
        {
            if (Is(State.Dragging))
            {
                var newTargetPosition = Input.mousePosition;
                var direction = (newTargetPosition - targetPosition).normalized;
                
                targetPosition = newTargetPosition;
            }
        }

        [Flags]
        public enum State
        {
            None = 0,
            
            /// <summary>
            /// 버려진 상태. 씬에 존재하지 않고 Pool에 들어감.
            /// </summary>
            Discarded = 1 << 0,
            
            /// <summary>
            /// 버려지고 있음. 아직 씬에 존재함.
            /// </summary>
            Discarding = 1 << 1,
            
            /// <summary>
            /// 리롤 중임.
            /// </summary>
            Rerolling = 1 << 2,
            
            /// <summary>
            /// 선택됐음. 
            /// </summary>
            Selected = 1 << 3,
            
            /// <summary>
            /// 드래그되고 있음.
            /// </summary>
            Dragging = 1 << 4,
            
            /// <summary>
            /// 본인을 포함해, 드래그되고 있는 카드가 있음.
            /// </summary>
            AnyCardDragging = 1 << 5,
            
            /// <summary>
            /// 카드 값 선택 중임.
            /// </summary>
            ChangingValue = 1 << 6,
        }

        public bool Is(State targetState)
        {
            return (_state & targetState) != 0;
        }

        public void Set(State targetState, bool value)
        {
            _state = (_state &= ~targetState) | (value ? targetState : 0);
            debugState = _state;
        }
    }
}