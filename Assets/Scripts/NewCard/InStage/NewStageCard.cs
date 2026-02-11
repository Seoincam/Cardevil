using Cardevil.Attributes;
using Cardevil.Core;
using Cardevil.NewCard.Core;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Cardevil.NewCard.InStage
{
    public interface IStageCard
    {
        ICardState State { get; }
    }
    
    public class NewStageCard : MonoBehaviour, IStageCard, IClearable, 
        IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, 
        IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [field: Header("State")]
        [field: SerializeReference, VisibleOnly] public ICardState State { get; private set; }

        public event Action<NewStageCard> PointerEnter;
        public event Action<NewStageCard> PointerDown;
        public event Action<NewStageCard> DragStart;
        public event Action<NewStageCard> Dragging;
        public event Action<NewStageCard> PointerUp;
        public event Action<NewStageCard> DragEnd;
        public event Action<NewStageCard> PointerExit;
        
        private Camera _cardCamera;

        /// <summary>
        /// 카드의 Local Position x 좌표.
        /// Swap 등 카드끼리 비교용으로 사용함.
        /// </summary>
        public float CurrentX => transform.localPosition.x;
        
        /// <summary>
        /// 드래그 되고 있는지 여부. Presenter가 업데이트함.
        /// </summary>
        public bool IsDragging { private get; set; }
        
        /// <summary>
        /// 카드가 손패 내에 존재할 때 목표 x 로컬 좌표.
        /// </summary>
        public float TargetLocalX { private get; set; }
        
        /// <summary>
        /// 카드가 손패 내에 존재할 때 목표 y 로컬 좌표.
        /// </summary>
        public float TargetLocalCurveY { private get; set; }
        
        /// <summary>
        /// 카드가 손패 내에 존재할 때 선택 상태로 더해지는 목표 y 로컬 좌표. 
        /// </summary>
        public float TargetSelectionY { private get; set; }
        
        /// <summary>
        /// 카드가 손패 내에 존재할 때 목표 z 로컬 커브.
        /// </summary>
        public float TargetCurveAngleZ { private get; set; }

        private Vector3 TargetLocalPosition
        {
            get
            {
                var x = TargetLocalX;
                var y = TargetLocalCurveY + TargetSelectionY;
                
                return new Vector3(x, y);
            }
        }

        // 드래그 중일 때 갱신되는 포인터의 위치.
        private Vector3 _targetPointerPosition;
        

        public void Initialize(ICardState cardState, Camera cardCamera)
        {
            // Clear();
            State = cardState;
            _cardCamera = cardCamera;
        }

        private void Update()
        {
            if (IsDragging)
            {
                Vector2 screen = Pointer.current.position.ReadValue();
                _targetPointerPosition = _cardCamera.ScreenToWorldPoint(
                    new Vector3(screen.x, screen.y, -_cardCamera.transform.position.z)
                );
            }
        }

        private void LateUpdate()
        {
            // TODO: 보간
            if (IsDragging)
            {
                transform.position = _targetPointerPosition;
                transform.localEulerAngles = Vector3.zero;
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, TargetLocalPosition, Time.deltaTime * 10);
                transform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(transform.localEulerAngles.z, TargetCurveAngleZ, Time.deltaTime * 10));
            }
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public void SetSortingOrder(int order)
        {
            spriteRenderer.sortingOrder = order;
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
        
        // 드래그 빼고는 모두 tween으로 움직이기.
    }
}