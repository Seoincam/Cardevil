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
                { VisualPhase.One, new SpriteOnePhaseState(visual) },
                { VisualPhase.Two, new SpriteTwoPhaseState(visual) },
                { VisualPhase.Three, new SpriteThreePhaseState(visual) }
            };
            _current = _states[VisualPhase.One];
            _current.OnEnter(spriteSet);
        }

        public async UniTaskVoid InitPhase(CardVisualSpriteSet spriteSet)
        {
            _current = _states[spriteSet.Phase];
            await _current.OnEnter(spriteSet);
        }

        public async UniTask SetPhase(CardVisualSpriteSet spriteSet)
        {
            await _current.OnExit();
            await _current.SetPhase(spriteSet.Phase);
            _current = _states[spriteSet.Phase];
            await _current.OnEnter(spriteSet);
        }
    }
}