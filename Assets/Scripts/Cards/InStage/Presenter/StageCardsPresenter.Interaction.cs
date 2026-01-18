using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.InStage
{
    public partial class StageCardsPresenter
    {
        private void BindCallback(Card card)
        {
            card.DragStart += OnDragStarted;
            card.Dragging += DetectSwapOnDragging;
            card.DragEnd += OnDragEnded;
            card.PointerDown += OnPointerDown;
            card.PointerUp += OnPointerUp;
        }

        private void UnbindCallback(Card card)
        {
            card.DragStart -= OnDragStarted;
            card.Dragging -= DetectSwapOnDragging;
            card.DragEnd -= OnDragEnded;
            card.PointerDown -= OnPointerDown;
            card.PointerUp -= OnPointerUp;
        }
        
        private void OnPointerDown(Card card)
        {
            if (!CanInteract) return;

            _model.UpdateInteractingInfo(card);
        }
        
        private void OnPointerUp(Card card)
        {
            Debug.Log("OnPointerUp");
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!CanInteract) return;

            bool changed = false;
            if (Time.time - _model.CurrentInteracting.LastInteractionTime < 2f) // !!! 시간 setting으로
            {
                if (_model.Selection.Contains(card))
                {
                    card.Set(Card.State.Selected, false);
                    card.MoveToSlotAsync().Forget();
                    changed = true;
                }
                else if (_model.Selection.Count < 4)
                {
                    _model.Select(card);
                    card.Set(Card.State.Selected, true);
                    changed = true;
                }
            }
            
            if (changed)
            {
                // TODO:
                // UI 로직
            }

            // OnDragEnded()보다 OnPointerUp()이 먼저 호출됨.
            if (!card.Is(Card.State.Dragging))
            {
                _model.ClearInteractingInfo();                
            }
        }
        
        private void OnDragStarted(Card card)
        {
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!_model.CurrentInteracting.Exist) return;

            foreach (var handCard in _model.Hand)
            {
                handCard.Set(Card.State.AnyCardDragging, true);   
            }
            card.Set(Card.State.Dragging, true);
        }

        private void OnDragEnded(Card card)
        {
            Debug.Log("OnDragEnded");
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!_model.CurrentInteracting.Exist) return;
            
            // TODO: 값 바꾸는 영역 내인가 체크
            if (false) return;
            
            foreach (var handCard in _model.Hand)
            {
                handCard.Set(Card.State.AnyCardDragging, false);   
            }
            card.Set(Card.State.Dragging, false);
            
            _model.ClearInteractingInfo();
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
        private void DetectSwapOnDragging(Card card)
        {
            if (!_model.CurrentInteracting.Exist) return;
            if (!_model.CurrentInteracting.Card.Is(Card.State.Dragging)) return;

            var dragging = _model.CurrentInteracting.Card;
            var draggingX = dragging.TargetPosition.x;
            if (!_model.TryGetIndexInHand(dragging, out var draggingIndex)) return;

            for (int otherIndex = 0; otherIndex < _model.Hand.Count; otherIndex++)
            {
                var other = _model.Hand[otherIndex];
                if (!other || other == dragging) continue;

                var otherX = other.Slot.position.x;
                // if ((draggingX > otherX && draggingIndex < otherIndex) ||
                //     (draggingX < otherX && draggingX > otherIndex))
                // {
                //     _model.SwapInHand(dragging, otherIndex);
                //     _view.UpdateAllCardsParentSlot();
                //     break;
                // }

                if (draggingX > otherX && draggingIndex < otherIndex)
                {
                    LogEx.Log($"Swap-드래그가 오른쪽으로 감. / draggingX: {draggingX} / otherX: {otherX}");
                    
                    _model.SwapInHand(draggingIndex, otherIndex);
                    _view.UpdateAllCardsParentSlot();
                    break;
                }

                if (draggingX < otherX && draggingIndex > otherIndex)
                {
                    LogEx.Log($"Swap-드래그가 왼쪽으로 감. / draggingX: {draggingX} / otherX: {otherX}");
                    
                    _model.SwapInHand(draggingIndex, otherIndex);
                    _view.UpdateAllCardsParentSlot();
                    break;
                }
            }
        }
        
        // 카드가 해당 위치에 따른 인덱스를 반환.
        private int GetIndexByPosition()
        {
            if (!_model.CurrentInteracting.Exist) return -1;

            var draggingX = _model.CurrentInteracting.Card.transform.position.x;

            for (int otherIndex = 0; otherIndex < _model.Hand.Count; otherIndex++)
            {
                var otherX = _model.Hand[otherIndex].transform.position.x;
                
                if (draggingX < otherX) return otherIndex;
            }

            return _model.Hand.Count;
        }
    }
}