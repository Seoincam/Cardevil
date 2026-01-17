using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Cardevil.Cards.Visual
{
    public sealed class CardVisualPhaseStateMachine
    {
        private readonly Dictionary<VisualPhase, IPhaseState> _states;
        private IPhaseState _current;

        public CardVisualPhaseStateMachine(ChangeableCardVisual visual, CardSpriteSet spriteSet)
        {
            _states = new Dictionary<VisualPhase, IPhaseState>()
            {
                { VisualPhase.One, new SpriteOnePhaseState(visual) },
                { VisualPhase.Two, new SpriteTwoPhaseState(visual) },
                { VisualPhase.Three, new SpriteThreePhaseState(visual) }
            };
            _current = _states[VisualPhase.One];
            _current.OnEnter(spriteSet);
        }

        public async UniTaskVoid InitPhase(CardSpriteSet spriteSet)
        {
            _current = _states[spriteSet.Phase];
            await _current.OnEnter(spriteSet);
        }

        public async UniTask SetPhase(CardSpriteSet spriteSet)
        {
            await _current.OnExit();
            await _current.SetPhase(spriteSet.Phase);
            _current = _states[spriteSet.Phase];
            await _current.OnEnter(spriteSet);
        }
    }
}