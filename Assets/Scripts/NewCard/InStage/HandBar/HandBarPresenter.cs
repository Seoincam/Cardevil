using Cardevil.NewCard.Common.Core;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class HandBarPresenter
    {
        [Header("States")]
        [SerializeField] private HandBarModel model = new();
        [field: SerializeField] public bool CanInteract { private get; set; }
        
        private HandBarView _view;

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
            
            _view.IsInValueSelectionZone(state);

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
            if (model.PointerDownData.Exists && Time.time - model.PointerDownData.LastInteractionTime > 0.2f) return;
            
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
            // TODO: 선택 가능하고, 영역 내에 있는지 체크

            if (_view.IsInValueSelectionZone(state))
            {
                
            }
            else if (model.DetachData.Exists)
            {
                // 손패 영역 밖이고, 선택 영영도 아니라면 원래 위치로 복귀.
                model.Insert(model.DragData.OriginalIndex, state);
                model.ClearDetachData();
                
                _view.ArrangeCards(model.Hand);
            }
            
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