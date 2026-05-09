using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableNumberElement : INumberElement
    {
        [SerializeField] private Optional<int> newNumber;
        
        public SelectableNumberElement()
        {
            newNumber = new Optional<int>(null);
        }
        
        public static SelectableNumberElement Fixed(int number) => new()
        {
            newNumber = new Optional<int>(number)
        };
        
        public static SelectableNumberElement Random() => new()
        {
            newNumber = new Optional<int>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableNumberElement { newNumber = newNumber };
        }

        public void Apply(CardStateBuilder builder)
        {
            builder.AddNumberAlternative(newNumber.HasValue ? newNumber.Value : null);
        }
    }
}