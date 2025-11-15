using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteTwoPhaseState : IPhaseState
    {
        private CardVisualBase _visual;
        private CardVisualBase.SelectionGroup _group;
        private const int Count = 2;
        
        public VisualPhase Kind => VisualPhase.SpriteTwo;

        public SpriteTwoPhaseState(CardVisualBase visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardVisualSpriteSet spriteSet)
        {
            _visual.InnerFrame.sprite = spriteSet.innerFrame;
            
            foreach (var g in _visual.SelectionGroups)
            {
                if (g.Count != Count) continue;
                _group = g;
                break;
            }
            
            _group.NumberGroup.SetActive(true);
            
            _group.NumberMap[CardVisualBase.SelectionGroup.Position.Top].sprite = spriteSet.sprites[0];
            _group.NumberMap[CardVisualBase.SelectionGroup.Position.Bottom].sprite = spriteSet.sprites[1];
            
            foreach (var image in _group.NumberMap.Values)
                image.rectTransform.localScale = Vector3.zero;
            
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(.75f, .5f));

            await seq;
        }
        
        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(0f, .5f));
            await seq;
            
            _group.NumberGroup.SetActive(true);
            // foreach (var image in _group.NumberMap.Values)
            //     image.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.SpriteOne:
                    await TransitToOneAsync(); break;
                case VisualPhase.SpriteThree:
                    await TransitToThreeAsync(); break;
                
                default: return;
            }
        }

        private async UniTask TransitToOneAsync()
        {
            
        }

        private async UniTask TransitToThreeAsync()
        {
            
        }
    }
}