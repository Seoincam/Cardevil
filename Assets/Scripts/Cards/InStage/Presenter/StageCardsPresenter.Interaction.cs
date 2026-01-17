using Cardevil.Cards.InStage.NCard;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace Cardevil.Cards.InStage.Presenter
{
    public partial class NewStageCardsPresenter
    {
        private void BindCallback(NewCard card)
        {
            card.DragStart += OnDragStarted;
            card.DragEnd += OnDragEnded;
            card.PointerDown += OnPointerDown;
            card.PointerUp += OnPointerUp;
        }

        private void UnbindCallback(NewCard card)
        {
            card.DragStart -= OnDragStarted;
            card.DragEnd -= OnDragEnded;
            card.PointerDown -= OnPointerDown;
            card.PointerUp -= OnPointerUp;
        }
        
        private void OnPointerDown(NewCard card)
        {
            if (!CanInteract) return;

            _model.RegisterInteractingCard(card);
        }
        
        private void OnPointerUp(NewCard card)
        {
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!CanInteract) return;

            bool changed = false;
            if (Time.time - _model.CurrentInteracting.LastInteractionTime < 2f) // !!! 시간 setting으로
            {
                if (_model.Selection.Contains(card))
                {
                    card.Set(NewCard.State.Selected, false);
                    card.MoveToSlotAsync().Forget();
                    changed = true;
                }
                else if (_model.Selection.Count < 4)
                {
                    _model.Select(card);
                    card.Set(NewCard.State.Selected, true);
                    changed = true;
                }
            }
            
            if (changed)
            {
                // TODO:
                // UI 로직
                // HandChanged Invoke
            }
            _model.ClearInteractingCard();
        }
        
        private void OnDragStarted(NewCard card)
        {
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!_model.CurrentInteracting.Exist) return;

            foreach (var handCard in _model.Hand)
            {
                handCard.Set(NewCard.State.AnyCardDragging, true);   
            }
            card.Set(NewCard.State.Dragging, true);
        }

        private void OnDragEnded(NewCard card)
        {
            Debug.Assert(_model.CurrentInteracting.Card == card);
            
            if (!_model.CurrentInteracting.Exist) return;
            
            // TODO: 값 바꾸는 영역 내인가 체크
            if (false) return;
            
            card.Set(NewCard.State.Dragging, false);
            _model.InsertHand(card, _model.CurrentInteracting.OriginalIndex);
        }

        // 드래그 중 핸드 영역을 벗어났을 때.
        private void OnExitHandWhileDragging()
        {
            if (!_model.CurrentInteracting.Exist) 
                return;
            
            _model.RemoveHand(_model.CurrentInteracting);
        }
        
        // 드래그 중 다시 핸드 영역에 진입했을 때.
        private void OnEnterHandWhileDragging()
        {
            if (!_model.CurrentInteracting.Exist)
                return;
            
            var toInsertIndex = GetIndexByPosition();
            _model.InsertHand(_model.CurrentInteracting, toInsertIndex);
            // TODO: HandChanged?.Invoke
        }

        // 드래그 중인 카드가 있다면, 매 틱마다 해당 카드의 x 좌표와
        // 손패의 다른 카드의 x 좌표를 비교하는 방식으로 Swap함.
        private void UpdateDetectSwap()
        {
            if (!_model.CurrentInteracting.Exist) return;
            if (!_model.CurrentInteracting.Card.Is(NewCard.State.Dragging)) return;

            var dragging = _model.CurrentInteracting.Card;
            var draggingX = dragging.transform.position.x;
            if (!_model.TryGetIndexInHand(dragging, out var draggingIndex)) return;

            for (int otherIndex = 0; otherIndex < _model.Hand.Count; otherIndex++)
            {
                var other = _model.Hand[otherIndex];
                if (!other || other == dragging) continue;
                
                var otherX = other.transform.position.x;
                if ((draggingX > otherX && draggingIndex < otherIndex) ||
                    (draggingX < otherX && draggingX > otherIndex))
                {
                    _model.SwapInHand(dragging, otherIndex);
                    // TODO: handChanged?.Invoke()
                    break;
                }
            }
        }
        
        // 카드가 해당 위치에 존재한다 가정하고 인덱스를 반환.
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