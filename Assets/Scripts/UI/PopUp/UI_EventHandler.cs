using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnPointerUpHandler = null;
    public Action<PointerEventData> BeginDragHandler = null;
    public Action<PointerEventData> DragHandler = null;
    public Action<PointerEventData> DragEndHandler = null;
    public Action<PointerEventData> OnPointerDownHandler = null;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (BeginDragHandler != null)
        {
            BeginDragHandler.Invoke(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragHandler != null)
        {
            DragHandler.Invoke(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragEndHandler != null)
        {
            DragEndHandler.Invoke(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
        {
            OnClickHandler.Invoke(eventData);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnPointerDownHandler != null)
        {
            OnPointerDownHandler.Invoke(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (OnPointerUpHandler != null)
        {
            OnPointerUpHandler.Invoke(eventData);
        }
    }
}
