using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public class BaseColorElement : ISpecElement
    {
        [SerializeField] private CardColor color;
        
        public BaseColorElement(CardColor defaultColor)
        {
            color = defaultColor;    
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultColor(color);
        }
    }
}