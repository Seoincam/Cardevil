using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeNumberElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<int> number;
        
        public static AlternativeNumberElement Fixed(int number) => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Fixed(number)
        };
        
        public static AlternativeNumberElement Random() => new()
        {
            number = CardStateBuilder.SelectableSlot<int>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberSelectableSlot(number);
        }
    }
}