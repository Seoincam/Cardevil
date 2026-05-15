using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableNumberElement : INumberElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<int> number;
        
        public SelectableNumberElement()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random();
        }
        
        public static SelectableNumberElement Fixed(int number) => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Fixed(number)
        };
        
        public static SelectableNumberElement Random() => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random()
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableNumberElement { number = number };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberSelectableSlot(number);
        }
    }
}