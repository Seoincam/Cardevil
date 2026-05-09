using Cardevil.Core;
using Cardevil.Core.Utils;
using System.Collections.Generic;

namespace Cardevil.Card.Common.Core
{
    /// <summary>
    /// <see cref="CardSpec"/>을 기반으로 <see cref="NewCardState"/>을 생성하는 빌더.
    /// </summary>
    public sealed class NewCardStateBuilder : IClearable
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

        public NewCardState Build(CardSpec spec)
        {
            Clear();
            foreach (var element in spec.Elements)
            {
                element.Apply(this);
            }

            var state = new NewCardState(spec);
            
            // 공격 카드인 경우는 Resolve를 하지 않고, 이동 카드는 이 시점에 미리 Resolve함.
            if (spec.Type == CardType.Attack)
            {
                state.ColorList = new NewCardState.ValueList<CardColor>(DefaultColor, _colorAlternatives);
                state.NumberList = new NewCardState.ValueList<int>(DefaultNumber, _numberAlternatives);
            }
            else if (spec.Type == CardType.Move)
            {
                if (_directionAlternatives.Count is 1)
                {
                    var resolvedDirection = SelectableSlotsResolver.ResolveDirections(
                        DefaultDirection,
                        _directionAlternatives
                    );
                    state.DirectionList = new NewCardState.ValueList<Direction>(null, resolvedDirection); // 아직 선택 안 했으니깐 defaultValue null로 넘김

                    state.DirectionFlag = DefaultDirection.Value.ToDirectionFlag();
                    state.DirectionFlag |= resolvedDirection[1].ToDirectionFlag();
                }
                else if (_directionAlternatives.Count == 4)
                {
                    var resolvedDirection = SelectableSlotsResolver.ResolveDirections(
                        DefaultDirection,
                        _directionAlternatives
                    );
                    state.DirectionList = new NewCardState.ValueList<Direction>(null, resolvedDirection);

                    state.DirectionFlag = DirectionFlag.All;
                }
                else
                {
                    state.DirectionList = new NewCardState.ValueList<Direction>(DefaultDirection);
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