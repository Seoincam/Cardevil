using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeColorElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<CardColor> color;
        
        public static AlternativeColorElement Fixed(CardColor color) => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Fixed(color)
        };
        
        public static AlternativeColorElement Random() => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorSelectableSlot(color);
        }
    }
}