using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.Cards.InStage
{
    public partial class StageCard
    {
        private Tween _hoverScaleTween;

        public event Action<StageCard> PointerDown;
        public event Action<StageCard> PointerUp;
        public event Action<StageCard> DragStart;
        public event Action<StageCard> Dragging;
        public event Action<StageCard> DragEnd;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!CanInteract) return;
            if (Is(State.Dragging)) return;
            
            _hoverScaleTween?.Kill();
            _hoverScaleTween = visualRoot
                .DOScale(visualSetting.hoverScale, visualSetting.hoverScaleTweenDuration)
                .SetEase(visualSetting.hoverEase);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Is(State.Dragging)) return;
            
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
            Dragging?.Invoke(this);

            var t = config.DragFollowSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, Input.mousePosition, t); // TODO: Input 접근 수정
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