using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeDirectionElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<Direction> direction;
        
        public static AlternativeDirectionElement Fixed(Direction direction) => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Fixed(direction)
        };
        
        public static AlternativeDirectionElement Random() => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionSelectableSlot(direction);
        }
    }
}