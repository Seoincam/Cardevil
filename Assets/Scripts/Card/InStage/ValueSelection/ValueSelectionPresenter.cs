using Cardevil.Card.Common.Core;
using Cardevil.Card.Common.Core.Upgrade;
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
        public void TryOpenValueSelectionZone(ICardState state)
        {
            if (state.UpgradePath == UpgradePath.None ||
                state.ValueSelected)
            {
                return;
            }
            
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

            if (state.ValueSelected) return false;

            switch (state.UpgradePath)
            {
                case UpgradePath.MultiColor:
                    OnColorSelectable(state, handBarCardId);
                    return true;
                
                case UpgradePath.MultiNumber:
                    OnNumberSelectable(state, handBarCardId);
                    return true;
                
                case UpgradePath.MultiDirection:
                    OnDirectionSelectable(state, handBarCardId);
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
            int number = state.NumberList.FixedValue;
            foreach (var color in state.ColorList.AllCandidateValues)
            {
                _view.CreateColorAlternative(color!.Value, number);
            }   
            
            _view.SetDimActive(true);
            _view.ArrangeCards(handBarCardId, state.ColorList.AllCandidateValues.Select(c => c!.Value).ToArray());
        }

        private void OnNumberSelectable(ICardState state, uint handBarCardId)
        {
            CardColor color = state.ColorList.FixedValue;
            foreach (var number in state.NumberList.AllCandidateValues)
            {
                _view.CreateNumberAlternative(color, number!.Value);
            }
            
            _view.SetDimActive(true);
            _view.ArrangeCards(handBarCardId, state.NumberList.AllCandidateValues.Select(n => n!.Value).ToArray());
        }

        private void OnDirectionSelectable(ICardState state, uint handBarCardId)
        {
            foreach (var direction in state.DirectionList.AllCandidateValues)
            {
                _view.CreateDirectionAlternative(direction!.Value);
            }
            
            _view.SetDimActive(true);
            _view.ArrangeCards(handBarCardId, state.DirectionList.AllCandidateValues.Select(d => d!.Value).ToArray());
        }

        private void OnValueSelected(in ValueSelectionView.Values values, uint cardId)
        {
            switch (_targetState.UpgradePath)
            {
                case UpgradePath.MultiColor:
                    _targetState.ColorList.Fix(values.Color);
                    break;
                
                case UpgradePath.MultiNumber:
                    _targetState.NumberList.Fix(values.Number);
                    break;
                
                case UpgradePath.MultiDirection:
                    _targetState.DirectionList.Fix(values.Direction); 
                    break;
            }
            
            _view.Clear(except: cardId);
            _view.SetDimActive(false);
            ValueSelected?.Invoke(_targetState, cardId);
            _targetState = null;
        }
    }
}