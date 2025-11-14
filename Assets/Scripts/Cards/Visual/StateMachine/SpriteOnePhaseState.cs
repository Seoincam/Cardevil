using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteOnePhaseState : IPhaseState
    {
        private CardVisualBase _visual;
        public VisualPhase Kind => VisualPhase.SpriteOne;

        public SpriteOnePhaseState(CardVisualBase visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardVisualSpriteSet spriteSet)
        {
            _visual.InnerFrame.sprite = spriteSet.innerFrame;
            _visual.Number.sprite = spriteSet.sprites[0];
            
            _visual.Number.gameObject.SetActive(true);
            await _visual.Number.rectTransform.DOScale(1f, .5f);
        }

        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            await _visual.Number.rectTransform.DOScale(0f, .5f);
            _visual.Number.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.SpriteTwo:
                    await TransitToTwoAsync(); break;
                case VisualPhase.SpriteThree:
                    await TransitToThreeAsync(); break;
                
                default: return;
            } 
        }

        private async UniTask TransitToTwoAsync()
        {
            
        }

        private async UniTask TransitToThreeAsync()
        {
            
        }
    }
}