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
using Random = UnityEngine.Random;

namespace Cardevil.NewCard.InStage
{
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
        public event Action<HandBarCard> PointerUp;
        public event Action<HandBarCard> DragEnd;
        public event Action<HandBarCard> PointerExit;
        
        private Camera _cardCamera;

        private ICardMovement _movement;
        private ICardRotation _rotation;
        
        // Movement Rotation
        private Vector3 _previousPosition;
        
        [SerializeField] private float rotationAmount = 10f;
        [SerializeField] private float maxTilt = 50f;
        
        public LocalPositionResolver LocalTargetPosition { get; private set; }
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
        
        private float MovementRotationZ
        {
            get
            {
                Vector3 velocity = (transform.position - _previousPosition) / Time.deltaTime;
                _previousPosition = transform.position;
                float targetZ = -velocity.x * rotationAmount;
                
                return Mathf.Clamp(targetZ, -maxTilt, maxTilt);
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
        
        public enum Mode
        {
            InHand,
            Dragging,
            WorldTarget,
            Unmanaged
        }

        public void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.InHand:
                    SetMovement(MovementType.LerpToLocal);
                    SetRotation(RotationType.LerpToLocalZ);
                    break;
                
                case Mode.Dragging:
                    SetMovement(MovementType.MoveToPointer);
                    SetRotation(RotationType.LerpWithMovement);
                    break;
                
                case Mode.WorldTarget:
                    SetMovement(MovementType.LerpToWorld);
                    SetRotation(RotationType.LerpToLocalZ);
                    break;
                
                case Mode.Unmanaged:
                    SetMovement(MovementType.None);
                    SetRotation(RotationType.None);
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
            // Debug.Log("OnPointerDown");
            PointerDown?.Invoke(this);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Debug.Log("OnBeginDrag");
            DragStart?.Invoke(this);
        }
        
        public void OnDrag(PointerEventData eventData) { }
        
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

        public async UniTask PlayDiscardAsync(Vector3 discardPosition, float duration, DiscardParams discardParams)
        {
            VisualController.SetTrail();

            var scaleTween = transform
                .DOScale(discardParams.TargetScale, duration)
                .SetEase(discardParams.ScaleEase);

            var rotationTween = transform
                .DOLocalRotate(discardParams.Rotation, duration)
                .SetEase(discardParams.RotationEase);

            float multiplier = Random.Range(discardParams.JumpPowerRandomRange.x, discardParams.JumpPowerRandomRange.y);
            float jumpPower = discardParams.JumpPower * multiplier;
            var jumpTween = transform
                .DOJump(discardPosition, jumpPower, 1, duration)
                .SetEase(discardParams.JumpEase);

            var fadeTween = VisualController.DoFade(0f, duration, discardParams.FadeEase);

            await DOTween.Sequence()
                .Join(scaleTween)
                .Join(rotationTween)
                .Join(jumpTween)
                .Join(fadeTween);
        }
        
        private void SetMovement(MovementType type)
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
                    _movement = new LerpToWorldPosition(type, () => WorldTargetPosition, 10f);
                    break;
                
                case MovementType.MoveToPointer:
                    _movement = new LerpToWorldPosition(type, () => PointerPosition, 20f);
                    break;
            }
        }

        private void SetRotation(RotationType type)
        {
            if (_rotation != null && _rotation.Type == type) return;

            switch (type)
            {
                case RotationType.None:
                    _rotation = null;
                    break;
                
                case RotationType.LerpToLocalZ:
                    _rotation = new LerpToLocalZRotation(type, () => TargetCurveAngleZ, 10f);
                    break;
                
                case RotationType.LerpWithMovement:
                    _rotation = new LerpToLocalZRotation(type, () => MovementRotationZ, 10f);
                    break;
            }
        }
    }
}