using Cardevil.Attributes;
using Cardevil.Core;
using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.Visual.Controller;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Cardevil.NewCard.InStage
{
    /*
     * [Position]
     * 드래그 모드(Mouse Pos),
     * 슬롯으로 돌아가는 모드(Local Position),
     * 특정 위치로 가는 모드(World Position),
     * 트윈으로 가는 모드(None)
     *
     * [Rotation]
     * 고정 (Lerp),
     * 트윈으로 제어 (None)
     */
    
    public class HandBarCard : MonoBehaviour, IClearable, 
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, 
        IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [field: SerializeField] public CardVisualController VisualController { get; private set; }
        
        [field: Header("State")]
        [field: SerializeReference, VisibleOnly] public ICardState State { get; private set; }

        public event Action<HandBarCard> PointerEnter;
        public event Action<HandBarCard> PointerDown;
        public event Action<HandBarCard> DragStart;
        public event Action<HandBarCard> Dragging;
        public event Action<HandBarCard> PointerUp;
        public event Action<HandBarCard> DragEnd;
        public event Action<HandBarCard> PointerExit;
        
        private Camera _cardCamera;

        private ICardMovement _movement;
        private ICardRotation _rotation;
        
        public LocalPositionResolver LocalTargetPosition { get; set; }
        public Vector3 WorldTargetPosition { get; set; }
        
        public float TargetCurveAngleZ { get; set; }

        /// <summary>
        /// 카드의 Local Position x 좌표.
        /// Swap 등 카드끼리 비교용으로 사용함.
        /// </summary>
        public float CurrentX => transform.localPosition.x;
        
        private Vector3 PointerPosition
        {
            get
            {
                Vector2 screen = Pointer.current.position.ReadValue();
                return _cardCamera.ScreenToWorldPoint(
                    new Vector3(screen.x, screen.y, -_cardCamera.transform.position.z)
                );
            }
        }

        private void Awake()
        {
            LocalTargetPosition = new LocalPositionResolver();
        }

        private void LateUpdate()
        {
            _movement?.UpdatePosition(transform, Time.deltaTime);
            _rotation?.UpdateRotation(transform, Time.deltaTime);
        }

        public void Initialize(ICardState cardState, Camera cardCamera)
        {
            // Clear();
            State = cardState;
            _cardCamera = cardCamera;
            
            VisualController.SetLayout(cardState);
        }
        

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
        
        public void SetSortingOrder(int order) => VisualController.SetSortingOrder(order);

        public void SetMovement(MovementType type)
        {
            if (_movement != null && _movement.Type == type) return;

            switch (type)
            {
                case MovementType.None:
                    _movement = null;
                    break;
                
                case MovementType.LerpToLocal:
                    _movement = new LerpToLocalMovement(() => LocalTargetPosition.Resolve(), 10f);
                    break;
                
                case MovementType.LerpToWorld:
                    _movement = new LerpToWorldPosition(() => WorldTargetPosition, 10f);
                    break;
                
                case MovementType.MoveToPointer:
                    _movement = new MoveToPointerMovement(() => PointerPosition);
                    break;
            }
        }

        public void SetRotation(RotationType type)
        {
            if (_rotation != null && _rotation.Type == type) return;

            switch (type)
            {
                case RotationType.None:
                    _rotation = null;
                    break;
                
                case RotationType.LerpToLocalZ:
                    _rotation = new LerpToLocalZRotation(() => TargetCurveAngleZ, 10f);
                    break;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Debug.Log("OnPointerEnter");
            PointerEnter?.Invoke(this);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // Debug.Log("OnP ointerDown");
            PointerDown?.Invoke(this);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Debug.Log("OnBeginDrag");
            DragStart?.Invoke(this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // Debug.Log("OnDrag");
            Dragging?.Invoke(this);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            // Debug.Log("OnPointerUp");
            PointerUp?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Debug.Log("OnEndDrag");
            DragEnd?.Invoke(this);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            // Debug.Log("OnPointerExit");
            PointerExit?.Invoke(this);
        }

        public void Hover()
        {
            Debug.Log("Hover Requested");
        }

        public void ScaleDown()
        {
            
        }

        public enum FollowTargetType
        {
            None,
            
            Pointer,
            LocalPosition,
            WorldPosition,
        }

        public async UniTask MoveToTargetAsync(Vector3 targetPosition, float speed, Ease ease)
        {
            await transform.DOMove(targetPosition, speed)
                .SetSpeedBased()
                .SetEase(ease);
        }
    }
}