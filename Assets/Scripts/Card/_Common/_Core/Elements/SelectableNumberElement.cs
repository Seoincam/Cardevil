using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableNumberElement : INumberElement
    {
        [SerializeField] private Optional<int> number;
        
        public SelectableNumberElement()
        {
            number = new Optional<int>(null);
        }
        
        public static SelectableNumberElement Fixed(int number) => new()
        {
            number = new Optional<int>(number)
        };
        
        public static SelectableNumberElement Random() => new()
        {
            number = new Optional<int>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableNumberElement { number = number };
        }

        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberAlternative(number.HasValue ? number.Value : null);
        }
    }
}