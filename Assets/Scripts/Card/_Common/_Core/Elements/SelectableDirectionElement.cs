using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableDirectionElement : IDirectionElement
    {
        [SerializeField] private Optional<Direction> newDirection;
        
        public SelectableDirectionElement()
        {
            newDirection = new Optional<Direction>(null);
        }
        
        public static SelectableDirectionElement Fixed(Direction direction) => new()
        {
            newDirection = new Optional<Direction>(direction)
        };
        
        public static SelectableDirectionElement Random() => new()
        {
            newDirection = new Optional<Direction>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableDirectionElement {  newDirection = newDirection };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionAlternative(newDirection.HasValue ? newDirection.Value : null);
        }
    }
}