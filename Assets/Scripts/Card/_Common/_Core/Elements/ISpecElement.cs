using Cardevil.Core;

namespace Cardevil.Card.Common.Core
{
    /// <summary>
    /// 카드 상태를 구성하는 Spec 요소.
    /// </summary>
    public interface ISpecElement : IDeepClonable<ISpecElement>
    {
        void Apply(CardStateBuilder builder);
        void Apply(NewCardStateBuilder builder);
    }
    
    public interface IColorElement : ISpecElement { }
    public interface INumberElement : ISpecElement { }
    public interface IDirectionElement : ISpecElement { }
}