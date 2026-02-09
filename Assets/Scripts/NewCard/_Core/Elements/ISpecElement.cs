namespace Cardevil.NewCard.Core
{
    public interface ISpecElement
    {
        void Apply(CardStateBuilder builder);
    }
}