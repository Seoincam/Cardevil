using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public class BaseColorElement : IColorElement
    {
        [SerializeField] private CardColor color;

        public BaseColorElement() { }
        public BaseColorElement(CardColor defaultColor)
        {
            color = defaultColor;    
        }
        
        public ISpecElement DeepClone()
        {
            return new BaseColorElement(color);
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultColor(color);
        }
    }
}