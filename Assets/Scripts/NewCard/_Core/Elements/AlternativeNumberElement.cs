using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeNumberElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.AlternativeSlot<int> number;
        
        public static AlternativeNumberElement Fixed(int number) => new()
        {
            number = CardStateBuilder.AlternativeSlot<int>.Fixed(number)
        };
        
        public static AlternativeNumberElement Random() => new()
        {
            number = CardStateBuilder.AlternativeSlot<int>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberAlternative(number);
        }
    }
}