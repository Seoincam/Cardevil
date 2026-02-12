using Cardevil.Utils.Directions;
using System;
using UnityEngine;

namespace Cardevil.NewCard.Common.Core
{
    [Serializable]
    public class BaseDirectionElement : ISpecElement
    {
        [SerializeField] private Direction direction;

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