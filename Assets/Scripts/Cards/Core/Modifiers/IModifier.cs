using Cardevil.Cards.Persistence;

namespace Cardevil.Cards.Core
{
    public interface IModifier : IModifierSaveLoad
    {
        ModifierType Type { get; }

        void Apply(CardData.Builder b);
    }
}