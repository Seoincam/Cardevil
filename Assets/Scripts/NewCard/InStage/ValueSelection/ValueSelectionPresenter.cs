using Cardevil.NewCard.Common.Core;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage.ValueSelection
{
    public class ValueSelectionPresenter
    {
        private readonly ValueSelectionView _view;
        
        private ICardState _targetState;

        private CardState.ValueSelectableType SelectableType => _targetState.SelectableType;

        public ValueSelectionPresenter(ValueSelectionView view)
        {
            _view = view;
        }

        public void TryOpenValueSelectionZone(ICardState state)
        {
            if (state.SelectableType == CardState.ValueSelectableType.None) return;
            
            _view.OpenValueSelectionZone();
        }
        
        public void CloseValueSelectionZone() => _view.CloseValueSelectionZone();
        
        public bool IsOnValueSelectionZone(Vector3 worldPosition) => _view.IsOnValueSelectionZone(worldPosition);

        /// <summary>
        /// 카드가 Value Selectable일 경우 값 선택 존을 엽니다.
        /// </summary>
        public void OpenValueSelection(ICardState state)
        {
            _targetState = state;

            switch (SelectableType)
            {
                case CardState.ValueSelectableType.Color:
                    OnColorSelectable(state);
                    break;
                
                case CardState.ValueSelectableType.Number:
                    OnNumberSelectable(state);
                    break;
                
                case CardState.ValueSelectableType.Direction:
                    OnDirectionSelectable(state);
                    break;
                
                default: throw new ArgumentException("선택 불가능한 카드임", nameof(state));
            }
        }

        public void CloseValueSelectionZone(ICardState state)
        {
            _view.CloseValueSelectionZone();
        }

        public void Clear()
        { 
            _targetState = null;
        }

        private void OnColorSelectable(ICardState state)
        {
            int number = state.Numbers.DefaultValue;
            foreach (var color in state.Colors.AllOptions)
            {
                _view.AddColorSelectable(color, number);
            }   
            
            _view.ArrangeCards(state.Colors.AllOptions.ToArray());
        }

        private void OnNumberSelectable(ICardState state)
        {
            CardColor color = state.Colors.DefaultValue;
            foreach (var number in state.Numbers.AllOptions)
            {
                _view.AddNumberSelectable(color, number);
            }
            
            _view.ArrangeCards(state.Numbers.AllOptions.ToArray());
        }

        private void OnDirectionSelectable(ICardState state)
        {
            foreach (var direction in state.Directions.AllOptions)
            {
                _view.AddDirectionSelectable(direction);
            }
            
            _view.ArrangeCards(state.Directions.AllOptions.ToArray());
        }
    }
}