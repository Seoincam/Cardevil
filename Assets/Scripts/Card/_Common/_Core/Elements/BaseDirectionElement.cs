using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public class BaseDirectionElement : IDirectionElement
    {
        [SerializeField] private Direction direction;

        public BaseDirectionElement() { }
        public BaseDirectionElement(Direction defaultDirection)
        {
            direction = defaultDirection;
        }
        
        public ISpecElement DeepClone()
        {
            return new BaseDirectionElement(direction);
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultDirection(direction);
        }
    }
}