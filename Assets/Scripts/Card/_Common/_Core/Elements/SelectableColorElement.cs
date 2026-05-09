using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableColorElement : IColorElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<CardColor> color;
        [SerializeField] private Optional<CardColor> newColor;

        public SelectableColorElement()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Random();
            newColor = new Optional<CardColor>(null);
        }
        public static SelectableColorElement Fixed(CardColor color) => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Fixed(color),
            newColor = new Optional<CardColor>(color)
        };
        
        public static SelectableColorElement Random() => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Random(),
            newColor = new Optional<CardColor>(null)
        };
        
        public ISpecElement DeepClone()
        {
            return new SelectableColorElement { color = color, newColor = newColor };
        }
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorSelectableSlot(color);
        }

        public void Apply(NewCardStateBuilder builder)
        {
            builder.AddColorAlternative(newColor.HasValue ? newColor.Value : null);
        }
    }
}