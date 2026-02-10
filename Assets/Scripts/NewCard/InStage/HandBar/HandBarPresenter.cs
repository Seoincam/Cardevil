using Cardevil.NewCard.Core;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class HandBarPresenter
    {
        [SerializeField] private HandBarModel model = new();
        
        private HandBarView _view;
        
        [field: SerializeField] public bool CanInteract { private get; set; }

        public HandBarPresenter(HandBarView view)
        {
            _view = view;
            
            _view.CardPointerEnter += OnPointerEnter;
            _view.CardPointerDown += OnPointerDown;
            _view.CardDragStart += OnDragStart;
            _view.CardDragging += OnDragging;
            _view.CardPointerUp += OnPointerUp;
            _view.CardDragEnd += OnDragEnd;
            _view.CardPointerExit += OnPointerExit;
        }

        public void AddCard(ICardState state)
        {
            model.Add(state);
            _view.CreateCard(state);
            _view.ArrangeCards(model.Hand);
        }

        public void RemoveCard(ICardState state)
        {
            model.Remove(state);
            _view.DestroyCard(state);
            _view.ArrangeCards(model.Hand);
        }

        private void OnPointerEnter(ICardState state)
        {
            if (!CanInteract) return;
        }

        private void OnPointerDown(ICardState state)
        {
            if (!CanInteract) return;
            
            model.SetPointerDownData(state);
        }

        private void OnDragStart(ICardState state)
        {
            if (!CanInteract) return;
            
            model.SetDraggingData(state);
            _view.StartDrag(state);
        }

        private void OnDragging(ICardState state)
        {
            if (!CanInteract) return;

            bool inZone = _view.IsInHandZone(state);

            if (model.DetachData.Exists && inZone)
            {
                // 핸드로 복귀
                model.Reattach(state);
            }
            else if (!model.DetachData.Exists && !inZone)
            {
                // 핸드에서 빠짐
                model.Detach(state);
                _view.ArrangeCards(model.Hand);
            }

            if (inZone)
            {
                TrySwap(state);   
            }
        }

        private void OnPointerUp(ICardState state)
        {
            if (!CanInteract) return;
            
            // 드래그를 했다면 시간도 체크함.
            if (model.PointerDownData.Exists && Time.time - model.PointerDownData.LastInteractionTime > 0.5f) return;
            
            if (model.Selection.Contains(state))
            {
                Debug.Log("Deselect");
                model.Deselect(state);
                _view.DeselectCard(state, model.Hand.Count, model.IndexOf(state));
            }
            else if (model.Selection.Count < 4)
            {
                Debug.Log("Select");
                model.Select(state);
                _view.SelectCard(state, model.Hand.Count, model.IndexOf(state));
            }
        }

        private void OnDragEnd(ICardState state)
        {
            model.ClearDraggingData();
            _view.EndDrag(state);
        }

        private void OnPointerExit(ICardState state)
        {
            if (!CanInteract) return;
        }

        private void TrySwap(ICardState dragging)
        {
            int from = model.IndexOf(dragging);
            float draggingX = _view.GetCurrentX(dragging);
            int count = model.Hand.Count;

            // 오른쪽으로 밀기
            while (from + 1 < count)
            {
                float nextX = _view.GetCurrentX(model.Hand[from + 1]);
                if (draggingX <= nextX) break;
        
                model.Swap(from, from + 1);
                from++;
            }

            // 왼쪽으로 밀기
            while (from - 1 >= 0)
            {
                float prevX = _view.GetCurrentX(model.Hand[from - 1]);
                if (draggingX >= prevX) break;
        
                model.Swap(from, from - 1);
                from--;
            }

            _view.ArrangeCards(model.Hand);
        }
    }
}