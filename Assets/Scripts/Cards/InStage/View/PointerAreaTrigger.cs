using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.InStage
{
    /// <summary>
    /// UI 영역 Hover 감지.
    /// 포인터 진입 및 이탈 시 이벤트 트리거.
    /// </summary>
    public class PointerAreaTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 포인터 진입 이벤트.
        /// </summary>
        public event Action PointerEntered;

        /// <summary>
        /// 포인터 이탈 이벤트.
        /// </summary>
        public event Action PointerExited;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke();
        }
    }
}