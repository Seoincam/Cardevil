using Cardevil.NewCard.Core;

namespace Cardevil.NewCard.InStage
{
    public class HandBarPresenter
    {
        private readonly HandBarModel _model = new();
        private readonly HandBarView _view;

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
            _model.Add(state);
            _view.CreateCard(state);
            _view.ArrangeCards(_model.Hand);
        }

        public void RemoveCard(ICardState state)
        {
            _model.Remove(state);
            _view.DestroyCard(state);
            _view.ArrangeCards(_model.Hand);
        }

        private void OnPointerEnter(ICardState state)
        {
            
        }

        private void OnPointerDown(ICardState state)
        {
            
        }

        private void OnDragStart(ICardState state)
        {
            
        }

        private void OnDragging(ICardState state)
        {
            TrySwap(state);
        }

        private void OnPointerUp(ICardState state)
        {
            
        }

        private void OnDragEnd(ICardState state)
        {
            
        }

        private void OnPointerExit(ICardState state)
        {
            
        }

        private void TrySwap(ICardState dragging)
        {
            int draggingIndex = _model.IndexOf(dragging);
            float draggingX = _view.GetCurrentX(dragging);

            for (int i = 0; i < _model.Hand.Count; i++)
            {
                if (i == draggingIndex) continue;
                
                float otherX = _view.GetCurrentX(_model.Hand[i]);
                
                bool shouldSwap = (draggingIndex < i && draggingX > otherX) ||
                                  (draggingIndex > i && draggingX < otherX);

                if (shouldSwap)
                {
                    _model.Swap(draggingIndex, i);
                    _view.ArrangeCards(_model.Hand);
                    break;
                }
            }
        }
    }
}