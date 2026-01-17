using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.InStage
{
    public partial class Card
    {
        public event Action<Card> PointerDown;
        public event Action<Card> PointerUp;
        public event Action<Card> DragStart;
        public event Action<Card> DragEnd;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract) return;
            
            _hoverScaleTween?.Kill();
            _hoverScaleTween = visualRoot
                .DOScale(visualSetting.hoverScale, visualSetting.hoverScaleTweenDuration)
                .SetEase(visualSetting.hoverEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverScaleTween?.Kill();
            _hoverScaleTween = visualRoot
                .DOScale(1f, visualSetting.hoverScaleTweenDuration)
                .SetEase(visualSetting.hoverEase);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CanInteract) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            PointerDown?.Invoke(this);
                
            visualRoot
                .DOScale(visualSetting.SelectScale, visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanInteract) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            DragStart?.Invoke(this);
            Set(State.Dragging, true);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!CanInteract) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            DragEnd?.Invoke(this);
            
            var endValue = Is(State.Selected) ? new Vector3(0, visualSetting.SelectOffset, 0) : Vector3.zero;
            var duration = visualSetting.EndDragTweenDuration;
            transform.DOLocalMove(endValue, duration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => Set(State.Dragging, false));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CanInteract) return;
            if (eventData.button != PointerEventData.InputButton.Left) return;
            
            PointerUp?.Invoke(this);

            visualRoot.DOScale(1f, visualSetting.SelectScaleTweenDuration)
                .SetEase(visualSetting.SelectScaleEase);
        }
    }
}