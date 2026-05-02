using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class BaseNumberElement : ISpecElement
    {
        [SerializeField] private int number;

        public BaseNumberElement() { }
        public BaseNumberElement(int defaultNumber)
        {
            number = defaultNumber;
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultNumber(number);
        }
    }
}