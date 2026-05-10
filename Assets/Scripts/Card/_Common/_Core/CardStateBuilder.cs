using Cardevil.Core;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;

namespace Cardevil.Card.Common.Core
{
    /// <summary>
    /// <see cref="CardSpec"/>을 기반으로 <see cref="CardState"/>을 생성하는 빌더.
    /// </summary>
    public sealed class CardStateBuilder : IClearable
    {
        // 기본값들.
        public CardColor? DefaultColor { get; set; }
        public int? DefaultNumber { get; set; }
        public Direction? DefaultDirection { get; set; }

        // 기본값 외 선택 가능한 값 목록들.
        private readonly List<CardColor?> _colorAlternatives = new();
        private readonly List<int?> _numberAlternatives = new();
        private readonly List<Direction?> _directionAlternatives = new();
        
        public void AddColorAlternative(CardColor? color) => _colorAlternatives.Add(color);
        public void AddNumberAlternative(int? number) => _numberAlternatives.Add(number);
        public void AddDirectionAlternative(Direction? direction) => _directionAlternatives.Add(direction);

        public CardState Build(CardSpec spec)
        {
            Clear();
            foreach (var element in spec.Elements)
            {
                element.Apply(this);
            }

            var state = new CardState(spec);
            
            // 공격 카드인 경우는 Resolve를 하지 않고, 이동 카드는 이 시점에 미리 Resolve함.
            if (spec.Type == CardType.Attack)
            {
                // Color
                if (!DefaultColor.HasValue)
                    throw new ArgumentNullException(nameof(DefaultColor), "기본색은 항상 존재해야합니다.");
                
                state.BaseColor = new Optional<CardColor>(DefaultColor);
                if (_colorAlternatives.Count == 0)
                {
                    state.ColorList = new CardState.ValueList<CardColor>(DefaultColor);
                }
                else if (_colorAlternatives.Count <= 3)
                {
                    var alternatives = new List<CardColor?> { DefaultColor };
                    alternatives.AddRange(_colorAlternatives);
                    state.ColorList = new CardState.ValueList<CardColor>(null, alternatives);
                }
                else
                {
                    var alternatives = new List<CardColor?>(_colorAlternatives);
                    state.ColorList = new CardState.ValueList<CardColor>(null, alternatives);
                }
                
                // Number
                state.NumberList = new CardState.ValueList<int>(DefaultNumber, _numberAlternatives);
            }
            else if (spec.Type == CardType.Move)
            {
                if (_directionAlternatives.Count is 1)
                {
                    var resolvedDirection = SelectableSlotsResolver.ResolveDirections(
                        DefaultDirection,
                        _directionAlternatives
                    );
                    state.DirectionList = new CardState.ValueList<Direction>(null, resolvedDirection); // 아직 선택 안 했으니깐 defaultValue null로 넘김

                    state.DirectionFlag = DefaultDirection.Value.ToDirectionFlag();
                    state.DirectionFlag |= resolvedDirection[1].ToDirectionFlag();
                }
                else if (_directionAlternatives.Count == 4)
                {
                    var resolvedDirection = SelectableSlotsResolver.ResolveDirections(
                        DefaultDirection,
                        _directionAlternatives
                    );
                    state.DirectionList = new CardState.ValueList<Direction>(null, resolvedDirection);

                    state.DirectionFlag = DirectionFlag.All;
                }
                else
                {
                    state.DirectionList = new CardState.ValueList<Direction>(DefaultDirection);
                    state.DirectionFlag = DefaultDirection.Value.ToDirectionFlag();
                }
            }

            return state;
        }

        public void Clear()
        {
            DefaultColor = null;
            DefaultNumber = null;
            DefaultDirection = null;

            _colorAlternatives.Clear();
            _numberAlternatives.Clear();
            _directionAlternatives.Clear();
        }
    }
}