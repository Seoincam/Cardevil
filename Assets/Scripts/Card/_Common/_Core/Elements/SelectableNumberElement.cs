using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableNumberElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<int> number;
        
        public static SelectableNumberElement Fixed(int number) => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Fixed(number)
        };
        
        public static SelectableNumberElement Random() => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberSelectableSlot(number);
        }
    }
}