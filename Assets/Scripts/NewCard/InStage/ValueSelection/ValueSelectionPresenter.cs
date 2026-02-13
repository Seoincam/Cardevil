using Cardevil.NewCard.Common.Core;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.NewCard.InStage.ValueSelection
{
    public class ValueSelectionPresenter
    {
        public event Action<ICardState> ValueSelected;
        
        private readonly ValueSelectionView _view;
        
        private ICardState _targetState;

        public Vector3 ZoneWorldPosition => _view.ZoneWorldPosition;
        
        private CardState.ValueSelectableType SelectableType => _targetState.SelectableType;

        public ValueSelectionPresenter(ValueSelectionView view)
        {
            _view = view;
            view.ValueSelected += OnValueSelected;
        }
        
        /// <summary>
        /// 값 선택 존 위에 있는가 여부를 반환.
        /// </summary>
        public bool IsOnValueSelectionZone(Vector3 worldPosition) => _view.IsOnValueSelectionZone(worldPosition);

        /// <summary>
        /// 카드가 Value Selectable일 경우 값 선택 존을 염.
        /// </summary>
        /// <param name="state"></param>
        public void TryOpenValueSelectionZone(ICardState state)
        {
            if (state.SelectableType == CardState.ValueSelectableType.None) return;
            
            _view.OpenValueSelectionZone();
        }
        
        /// <summary>
        /// 값 선택 존을 닫음.
        /// </summary>
        public void CloseValueSelectionZone() => _view.CloseValueSelectionZone();

        /// <summary>
        /// 카드가 Value Selectable일 경우 값 선택창을 염.
        /// </summary>
        public bool TryOpenValueSelection(ICardState state)
        {
            _targetState = state;

            switch (SelectableType)
            {
                case CardState.ValueSelectableType.Color:
                    OnColorSelectable(state);
                    return true;
                
                case CardState.ValueSelectableType.Number:
                    OnNumberSelectable(state);
                    return true;
                
                case CardState.ValueSelectableType.Direction:
                    OnDirectionSelectable(state);
                    return true;
            }
            
            return false;
        }

        public void CloseValueSelection()
        {
            _targetState = null;
            _view.Clear();
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

        private void OnValueSelected(in ValueSelectionView.Values values)
        {
            switch (SelectableType)
            {
                case CardState.ValueSelectableType.Color:
                    _targetState.Colors.Select(values.Color);
                    break;
                
                case CardState.ValueSelectableType.Number:
                    _targetState.Numbers.Select(values.Number);
                    break;
                
                case CardState.ValueSelectableType.Direction:
                    _targetState.Directions.Select(values.Direction);
                    break;
            }
            
            _view.Clear();
            ValueSelected?.Invoke(_targetState);
            _targetState = null;
        }
    }
}