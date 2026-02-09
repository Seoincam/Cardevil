using System;
using UnityEngine;

namespace Cardevil.NewCard.Core
{
    [Serializable]
    public sealed class AlternativeColorElement : ISpecElement
    {
        [SerializeField] private CardStateBuilder.AlternativeSlot<CardColor> color;
        
        public static AlternativeColorElement Fixed(CardColor color) => new()
        {
            color = CardStateBuilder.AlternativeSlot<CardColor>.Fixed(color)
        };
        
        public static AlternativeColorElement Random() => new()
        {
            color = CardStateBuilder.AlternativeSlot<CardColor>.Random()
        };
        
        public void Apply(CardStateBuilder builder)
        {
            builder.AddColorAlternative(color);
        }
    }
}