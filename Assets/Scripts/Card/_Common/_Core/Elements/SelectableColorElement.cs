using System;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public sealed class SelectableColorElement : IColorElement
    {
        [SerializeField] private CardStateBuilder.SelectableSlot<CardColor> color;

        public SelectableColorElement()
        {
            color = CardStateBuilder.SelectableSlot<CardColor>.Random();
        }
        
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