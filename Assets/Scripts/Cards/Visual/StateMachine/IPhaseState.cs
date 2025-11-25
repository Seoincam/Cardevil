using Cysharp.Threading.Tasks;

namespace Cardevil.Cards.Visual.StateMachine
{
    public interface IPhaseState
    {
        VisualPhase Kind { get; }

        UniTask OnEnter(CardVisualSpriteSet spriteSet);
        UniTask OnExit();
        UniTask SetPhase(VisualPhase phase);
    }
}