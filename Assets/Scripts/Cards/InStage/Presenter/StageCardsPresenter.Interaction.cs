using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    public partial class StageCardsPresenter
    {
        private void BindCallback(StageCard card)
        {
            card.DragStart += OnDragStarted;
            card.Dragging += DetectSwapOnDragging;
            card.DragEnd += OnDragEnded;
            card.PointerDown += OnPointerDown;
            card.PointerUp += OnPointerUp;
        }

        private void UnbindCallback(StageCard card)
        {
            card.DragStart -= OnDragStarted;
            card.Dragging -= DetectSwapOnDragging;
            card.DragEnd -= OnDragEnded;
            card.PointerDown -= OnPointerDown;
            card.PointerUp -= OnPointerUp;
        }
        
        private void OnPointerDown(StageCard card)
        {
            if (!CanInteract) return;

            _model.UpdateInteractingInfo(card);
        }
        
        private void OnPointerUp(StageCard card)
        {
            Debug.Assert(_model.CurrentInteracting.StageCard == card);
            
            if (!CanInteract) return;

            bool changed = false;
            if (Time.time - _model.CurrentInteracting.LastInteractionTime < 2f) // !!! 시간 setting으로
            {
                if (_model.Selection.Contains(card))
                {
                    _model.Deselect(card);
                    card.Set(StageCard.State.Selected, false);
                    changed = true;
                }
                else if (_model.Selection.Count < 4)
                {
                    _model.Select(card);
                    card.Set(StageCard.State.Selected, true);
                    changed = true;
                }
            }
            
            if (changed)
            {
                // TODO:
                // UI 로직
            }

            // OnDragEnded()보다 OnPointerUp()이 먼저 호출됨.
            if (!card.Is(StageCard.State.Dragging))
            {
                _model.ClearInteractingInfo();                
            }
        }
        
        private void OnDragStarted(StageCard card)
        {
            Debug.Assert(_model.CurrentInteracting.StageCard == card);
            
            if (!_model.CurrentInteracting.Exist) return;

            foreach (var handCard in _model.Hand)
            {
                handCard.Set(StageCard.State.AnyCardDragging, true);   
            }
            card.Set(StageCard.State.Dragging, true);
        }

        private void OnDragEnded(StageCard card)
        {
            Debug.Log("OnDragEnded");
            Debug.Assert(_model.CurrentInteracting.StageCard == card);
            
            if (!_model.CurrentInteracting.Exist) return;
            
            // TODO: 값 바꾸는 영역 내인가 체크
            if (false) return;
            
            // 핸드바 영역 내에서 드래그가 끝났다면
            foreach (var handCard in _model.Hand)
            {
                handCard.Set(StageCard.State.AnyCardDragging, false);   
            }
            card.Set(StageCard.State.Dragging, false);
            
            _model.ClearInteractingInfo();
            // card.MoveToSlotAsync().Forget();
        }

        // 드래그 중 핸드 영역을 벗어났을 때.
        private void OnExitHandWhileDragging()
        {
            if (!_model.CurrentInteracting.Exist) 
                return;
            
            _model.RemoveHand(_model.CurrentInteracting);
            _view.SetCardParentTemp(_model.CurrentInteracting);
            _view.UpdateAllCardsParentSlot();
        }
        
        // 드래그 중 다시 핸드 영역에 진입했을 때.
        private void OnEnterHandWhileDragging()
        {
            if (!_model.CurrentInteracting.Exist)
                return;
            
            var toInsertIndex = GetIndexByPosition();
            _model.InsertHand(_model.CurrentInteracting, toInsertIndex);
            
            _view.UpdateAllCardsParentSlot();
        }

        // 드래그 중인 카드가 있다면, 매 틱마다 해당 카드의 x 좌표와
        // 손패의 다른 카드의 x 좌표를 비교하는 방식으로 Swap함.
        private void DetectSwapOnDragging(StageCard card)
        {
            if (!_model.CurrentInteracting.Exist) return;
            if (!_model.CurrentInteracting.StageCard.Is(StageCard.State.Dragging)) return;

            var dragging = _model.CurrentInteracting.StageCard;
            var draggingX = dragging.transform.position.x;
            if (!_model.TryGetIndexInHand(dragging, out var draggingIndex)) return;

            for (int otherIndex = 0; otherIndex < _model.Hand.Count; otherIndex++)
            {
                var other = _model.Hand[otherIndex];
                if (!other || other == dragging) continue;

                var otherX = other.Slot.position.x;
                if ((draggingX > otherX && draggingIndex < otherIndex) ||
                    (draggingX < otherX && draggingIndex > otherIndex))
                {
                    _model.SwapInHand(dragging, otherIndex);
                    _view.UpdateAllCardsParentSlot();
                    break;
                }
            }
        }
        
        // 카드가 해당 위치에 따른 인덱스를 반환.
        private int GetIndexByPosition()
        {
            if (!_model.CurrentInteracting.Exist) return -1;

            var draggingX = _model.CurrentInteracting.StageCard.transform.position.x;

            for (int otherIndex = 0; otherIndex < _model.Hand.Count; otherIndex++)
            {
                var otherX = _model.Hand[otherIndex].transform.position.x;
                
                if (draggingX < otherX) return otherIndex;
            }

            return _model.Hand.Count;
        }
    }
}