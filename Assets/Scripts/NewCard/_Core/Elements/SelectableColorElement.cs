using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class SelectableColorElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<CardColor> color;
        
        public static SelectableColorElement Fixed(CardColor color) => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Fixed(color)
        };
        
        public static SelectableColorElement Random() => new()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorSelectableSlot(color);
        }
    }
}