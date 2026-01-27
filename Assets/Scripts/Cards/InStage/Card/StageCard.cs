using Cardevil.Attributes;
using Cardevil.Cards.Config;
using Cardevil.Cards.Config.StageCard;
using Cardevil.Cards.Core;
using Cardevil.Cards.Visual;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cardevil.Cards.InStage
{
    public partial class StageCard : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, 
        IPointerUpHandler, IPointerDownHandler,
        IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Config")]
        [SerializeField, VisibleOnly(EditableIn.EditMode)] 
        private CardVisualSettingSO visualSetting;
        
        [SerializeField, VisibleOnly(EditableIn.EditMode)] 
        private CardChangeValueFlipSetting changeFlipSetting;
        
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private StageCardDefaultConfig config;

        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private StageCardRerollConfig rerollConfig;
        
        [SerializeField, VisibleOnly(EditableIn.EditMode)]
        private CurveConfig curveConfig;

        [field: Header("Data")] 
        [field: SerializeField, VisibleOnly] public CardData Data { get; private set; }

        [field: Header("State")]
        [SerializeField] private State debugState; // VisibleOnly일 경우 enum을 확인할 수 없어, 디버깅용으로 추가함.
        
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
        
        public static implicit operator CardData(StageCard stageCard) => stageCard.Data;

        public Transform Slot => transform.parent;
        
        private bool HasParent => transform.parent;

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
            
            CalculateCurveInHand();
            LerpMoveToSlot();
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
            
            /// <summary>
            /// 트윈 중임
            /// </summary>
            Tweening = 1 << 7
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