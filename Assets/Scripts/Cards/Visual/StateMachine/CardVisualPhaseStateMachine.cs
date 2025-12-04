using Cardevil.Cards.Visual.Base;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Cardevil.Cards.Visual.StateMachine
{
    public sealed class CardVisualPhaseStateMachine
    {
        private readonly Dictionary<VisualPhase, IPhaseState> _states;
        private IPhaseState _current;

        public CardVisualPhaseStateMachine(CardVisualBase visual, CardVisualSpriteSet spriteSet)
        {
            _states = new Dictionary<VisualPhase, IPhaseState>()
            {
                { VisualPhase.SpriteOne, new SpriteOnePhaseState(visual) },
                { VisualPhase.SpriteTwo, new SpriteTwoPhaseState(visual) },
                { VisualPhase.SpriteThree, new SpriteThreePhaseState(visual) }
            };
            _current = _states[VisualPhase.SpriteOne];
            _current.OnEnter(spriteSet);
        }

        public async UniTaskVoid InitPhase(VisualPhase phase, CardVisualSpriteSet spriteSet)
        {
            _current = _states[phase];
            await _current.OnEnter(spriteSet);
        }

        public async UniTask SetPhase(VisualPhase phase, CardVisualSpriteSet spriteSet)
        {
            await _current.OnExit();
            await _current.SetPhase(phase);
            _current = _states[phase];
            await _current.OnEnter(spriteSet);
        }
    }
}