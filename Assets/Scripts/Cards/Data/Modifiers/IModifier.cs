using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Data.Save;
using Cardevil.Save;
using System;

namespace Cardevil.Cards.Data.Modifiers
{
    public interface IModifier : IModifierSaveLoad
    {
        ModifierType Type { get; }

        void Apply(CardData.Builder b);
    }
}