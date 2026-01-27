using Cysharp.Threading.Tasks;

namespace Cardevil.Cards.Visual
{
    public interface IPhaseState
    {
        VisualPhase Kind { get; }

        UniTask OnEnter(CardSpriteSet spriteSet);
        UniTask OnExit();
        UniTask SetPhase(VisualPhase phase);
    }
}