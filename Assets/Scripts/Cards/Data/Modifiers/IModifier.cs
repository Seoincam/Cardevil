using Cardevil.Cards.Data.InStage;
using System;

namespace Cardevil.Cards.Data.Modifiers
{
    public interface IModifier
    {
        ModifierType Type { get; }

        void Apply(CardData.Builder b);
    }
}