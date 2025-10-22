using Cardevil.Cards.Data.InStage;

namespace Cardevil.Cards.Data.Modifiers
{
    public interface IModifier
    {
        ModifierType Type { get; }

        void Apply(CardData.Builder b);
    }
}