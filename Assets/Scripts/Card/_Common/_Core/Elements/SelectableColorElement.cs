using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableColorElement : IColorElement
    {
        [SerializeField] private Optional<CardColor> newColor;

        public SelectableColorElement()
        {
            newColor = new Optional<CardColor>(null);
        }
        public static SelectableColorElement Fixed(CardColor color) => new()
        {
            newColor = new Optional<CardColor>(color)
        };
        
        public static SelectableColorElement Random() => new()
        {
            newColor = new Optional<CardColor>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableColorElement { newColor = newColor };
        }

        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorAlternative(newColor.HasValue ? newColor.Value : null);
        }
    }
}