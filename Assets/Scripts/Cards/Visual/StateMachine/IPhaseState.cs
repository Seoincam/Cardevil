using Cardevil.Cards.Visual.Sprites;
using Cysharp.Threading.Tasks;

namespace Cardevil.Cards.Visual.StateMachine
{
    public interface IPhaseState
    {
        VisualPhase Kind { get; }

        UniTask OnEnter(CardSpriteSet spriteSet);
        UniTask OnExit();
        UniTask SetPhase(VisualPhase phase);
    }
}