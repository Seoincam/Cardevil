using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public class BaseDirectionElement : ISpecElement
    {
        [SerializeField] private Direction direction;

        public BaseDirectionElement() { }
        public BaseDirectionElement(Direction defaultDirection)
        {
            direction = defaultDirection;
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultDirection(direction);
        }
    }
}