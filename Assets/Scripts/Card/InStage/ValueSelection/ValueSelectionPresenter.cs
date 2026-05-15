using Cardevil.Card.Common.Core;
using System;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    public class ValueSelectionPresenter
    {
        /// <summary>
        /// 타겟 ICardState와 Registry 상의 InteractionCard Id를 반환.
        /// </summary>
        public event Action<ICardState, uint> ValueSelected;
        
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
        public bool TryOpenValueSelection(ICardState state, uint handBarCardId)
        {
            _targetState = state;

            switch (SelectableType)
            {
                case CardState.ValueSelectableType.Color:
                    OnColorSelectable(state, handBarCardId);
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

        private void OnColorSelectable(ICardState state, uint handBarCardId)
        {
            int number = state.Numbers.DefaultValue;
            foreach (var color in state.Colors.AllOptions)
            {
                _view.AddColorSelectable(color, number);
            }   
            
            _view.SetDimActive(true);
            _view.ArrangeCards(state.Colors.AllOptions.ToArray(), handBarCardId);
        }

        private void OnNumberSelectable(ICardState state)
        {
            CardColor color = state.Colors.DefaultValue;
            foreach (var number in state.Numbers.AllOptions)
            {
                _view.AddNumberSelectable(color, number);
            }
            
            _view.SetDimActive(true);
            _view.ArrangeCards(state.Numbers.AllOptions.ToArray());
        }

        private void OnDirectionSelectable(ICardState state)
        {
            foreach (var direction in state.Directions.AllOptions)
            {
                _view.AddDirectionSelectable(direction);
            }
            
            _view.SetDimActive(true);
            _view.ArrangeCards(state.Directions.AllOptions.ToArray());
        }

        private void OnValueSelected(in ValueSelectionView.Values values, uint cardId)
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
            
            _view.Clear(except: cardId);
            _view.SetDimActive(false);
            ValueSelected?.Invoke(_targetState, cardId);
            _targetState = null;
        }
    }
}