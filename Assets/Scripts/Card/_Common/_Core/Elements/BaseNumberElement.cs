using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class BaseNumberElement : INumberElement
    {
        [SerializeField] private int number;

        public BaseNumberElement() { }
        public BaseNumberElement(int defaultNumber)
        {
            number = defaultNumber;
        }
        
        public ISpecElement DeepClone()
        {
            return new BaseNumberElement(number);
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.SetDefaultNumber(number);
        }
    }
}