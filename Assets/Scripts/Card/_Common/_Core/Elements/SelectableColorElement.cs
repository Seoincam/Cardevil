using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableColorElement : IColorElement
    {
        [SerializeField] private Optional<CardColor> color;

        public SelectableColorElement()
        {
            color = new Optional<CardColor>(null);
        }
        public static SelectableColorElement Fixed(CardColor color) => new()
        {
            color = new Optional<CardColor>(color)
        };
        
        public static SelectableColorElement Random() => new()
        {
            color = new Optional<CardColor>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableColorElement { color = color };
        }

        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorAlternative(color.HasValue ? color.Value : null);
        }
    }
}