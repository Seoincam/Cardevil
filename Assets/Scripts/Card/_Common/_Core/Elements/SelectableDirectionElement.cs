using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableDirectionElement : IDirectionElement
    {
        [SerializeField] private Optional<Direction> direction;
        
        public SelectableDirectionElement()
        {
            direction = new Optional<Direction>(null);
        }
        
        public static SelectableDirectionElement Fixed(Direction direction) => new()
        {
            direction = new Optional<Direction>(direction)
        };
        
        public static SelectableDirectionElement Random() => new()
        {
            direction = new Optional<Direction>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableDirectionElement {  direction = direction };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddDirectionAlternative(direction.HasValue ? direction.Value : null);
        }
    }
}