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
        [field: SerializeReference, VisibleOnly] public ICardState State { get; private set; }

        public event Action<NewStageCard> PointerEnter;
        public event Action<NewStageCard> PointerDown;
        public event Action<NewStageCard> DragStart;
        public event Action<NewStageCard> Dragging;
        public event Action<NewStageCard> PointerUp;
        public event Action<NewStageCard> DragEnd;
        public event Action<NewStageCard> PointerExit;
        
        private Camera _cardCamera;

        public float CurrentX => transform.localPosition.x;
        
        /// <summary>
        /// 드래그 되고 있는지 여부. Presenter가 업데이트함.
        /// </summary>
        public bool IsDragging { private get; set; }
        
        /// <summary>
        /// View에서 설정된 로컬 타겟. 기존 Slot과 같은 역할을 함.
        /// 아무런 상호작용이 없다면 해당 위치로 이동함.
        /// </summary>
        public Vector3 LocalTarget { private get; set; }
        
        /// <summary>
        /// Presenter에서 설정되는 상호작용 타겟.
        /// 마우스 등으로 움직일 때 해당 위치로 움직임.
        /// </summary>
        public Vector3 InteractingTarget { private get; set; }

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
                InteractingTarget = _cardCamera.ScreenToWorldPoint(
                    new Vector3(screen.x, screen.y, -_cardCamera.transform.position.z)
                );
            }
        }

        private void LateUpdate()
        {
            // TODO: 보간
            if (IsDragging)
            {
                transform.position = InteractingTarget;
            }
            else
            {
                transform.localPosition = LocalTarget;
            }
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
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