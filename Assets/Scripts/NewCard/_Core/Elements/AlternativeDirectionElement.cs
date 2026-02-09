using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeDirectionElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.AlternativeSlot<Direction> direction;
        
        public static AlternativeDirectionElement Fixed(Direction direction) => new()
        {
            direction = CardStateBuilder.AlternativeSlot<Direction>.Fixed(direction)
        };
        
        public static AlternativeDirectionElement Random() => new()
        {
            direction = CardStateBuilder.AlternativeSlot<Direction>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionAlternative(direction);
        }
    }
}