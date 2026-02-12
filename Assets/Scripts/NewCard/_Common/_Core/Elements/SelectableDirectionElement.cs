using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.NewCard.Common.Core
{
    [Serializable]
    public sealed class SelectableDirectionElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<Direction> direction;
        
        public static SelectableDirectionElement Fixed(Direction direction) => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Fixed(direction)
        };
        
        public static SelectableDirectionElement Random() => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionSelectableSlot(direction);
        }
    }
}