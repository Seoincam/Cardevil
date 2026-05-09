using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableDirectionElement : IDirectionElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<Direction> direction;
        [SerializeField] private Optional<Direction> newDirection;
        
        public SelectableDirectionElement()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Random();
            newDirection = new Optional<Direction>(null);
        }
        
        public static SelectableDirectionElement Fixed(Direction direction) => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Fixed(direction),
            newDirection = new Optional<Direction>(direction)
        };
        
        public static SelectableDirectionElement Random() => new()
        {
            direction = CardStateBuilder.SelectableSlot<Direction>.Random(),
            newDirection = new Optional<Direction>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableDirectionElement() { direction =  direction, newDirection = newDirection };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionSelectableSlot(direction);
        }

        public void Apply(NewCardStateBuilder builder)
        {
            builder.AddDirectionAlternative(newDirection);
        }
    }
}