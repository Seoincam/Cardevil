using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableNumberElement : INumberElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<int> number;
        [SerializeField] private Optional<int> newNumber;
        
        public SelectableNumberElement()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random();
            newNumber = new Optional<int>(null);
        }
        
        public static SelectableNumberElement Fixed(int number) => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Fixed(number),
            newNumber = new Optional<int>(number)
        };
        
        public static SelectableNumberElement Random() => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random(),
            newNumber = new Optional<int>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableNumberElement { number = number, newNumber = newNumber };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberSelectableSlot(number);
        }

        public void Apply(NewCardStateBuilder builder)
        {
            builder.AddNumberAlternative(newNumber.HasValue ? newNumber.Value : null);
        }
    }
}