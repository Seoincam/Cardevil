namespace Cardevil.NewCard.Core
{
    /// <summary>
    /// 카드 상태를 구성하는 Spec 요소.
    /// </summary>
    public interface ISpecElement
    {
        void Apply(CardStateBuilder builder);
    }
}