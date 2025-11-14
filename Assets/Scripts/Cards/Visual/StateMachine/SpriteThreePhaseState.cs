using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteThreePhaseState : IPhaseState
    {
        private CardVisualBase _visual;
        private CardVisualBase.SelectionGroup _group;
        private const int Count = 3;
        
        public VisualPhase Kind => VisualPhase.SpriteTwo;

        public SpriteThreePhaseState(CardVisualBase visual)
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

            _group.NumberMap[CardVisualBase.SelectionGroup.Position.Top].sprite = spriteSet.sprites[0];
            _group.NumberMap[CardVisualBase.SelectionGroup.Position.Middle].sprite = spriteSet.sprites[1];
            _group.NumberMap[CardVisualBase.SelectionGroup.Position.Bottom].sprite = spriteSet.sprites[2];
            
            foreach (var image in _group.NumberMap.Values)
                image.gameObject.SetActive(true);
            
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(1f, .5f));

            await seq;
        }
        
        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(0f, .5f));
            await seq;
            
            foreach (var image in _group.NumberMap.Values)
                image.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.SpriteOne:
                    await TransitToOneAsync(); break;
                case VisualPhase.SpriteTwo:
                    await TransitToTwoAsync(); break;
                
                default: return;
            }
        }

        private async UniTask TransitToOneAsync()
        {
            
        }
        
        private async UniTask TransitToTwoAsync()
        {
            
        }
    }
}