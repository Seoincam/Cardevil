using Cardevil.Cards.Visual.Base;
using Cardevil.Cards.Visual.Sprites;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteThreePhaseState : IPhaseState
    {
        private readonly ChangeableCardVisual _visual;
        private ChangeableCardVisual.PhaseGroup _group;
        
        public VisualPhase Kind => VisualPhase.Two;
        
        private RectTransform Middle => _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Middle];
        private RectTransform Bottom => _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Bottom];

        public SpriteThreePhaseState(ChangeableCardVisual visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardSpriteSet spriteSet)
        {
            _visual.InnerFrame.sprite = spriteSet.InnerFrame;

            _group = _visual.SelectionGroups[VisualPhase.Three];
            _group.Group.SetActive(true);

            _group.NumberMap[ChangeableCardVisual.Position.Top].sprite = spriteSet.MainSprites[0];
            _group.NumberMap[ChangeableCardVisual.Position.Middle].sprite = spriteSet.MainSprites[1];
            _group.NumberMap[ChangeableCardVisual.Position.Bottom].sprite = spriteSet.MainSprites[2];

            foreach (var image in _group.NumberMap.Values)
                image.rectTransform.localScale = Vector3.zero;
            
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(.6f, .5f));

            await seq;
        }
        
        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            var seq = DOTween.Sequence();
            foreach (var image in _group.NumberMap.Values)
                seq.Join(image.rectTransform.DOScale(0f, .5f));
            await seq;
            
            _group.Group.SetActive(false);
            // foreach (var image in _group.NumberMap.Values)
            //     image.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.One:
                    await TransitToOneAsync(); break;
                case VisualPhase.Two:
                    await TransitToTwoAsync(); break;
                
                default: return;
            }
        }

        private async UniTask TransitToOneAsync()
        {
            var middleInit =  ChangeableCardVisual.BackgroundPositions.MiddleInit;
            var bottomInit = ChangeableCardVisual.BackgroundPositions.BottomInit;
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(Middle.DOAnchorPos(middleInit, dur))
                .Join(Bottom.DOAnchorPos(bottomInit, dur));
            
            Middle.gameObject.SetActive(false);
            Bottom.gameObject.SetActive(false);
        }
        
        private async UniTask TransitToTwoAsync()
        {
            var middleFinal =  ChangeableCardVisual.BackgroundPositions.MiddleFinal;
            var bottomInit =  ChangeableCardVisual.BackgroundPositions.BottomInit;
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(Middle.DOAnchorPos(middleFinal, dur))
                .Join(Bottom.DOAnchorPos(bottomInit, dur));
            
            Bottom.gameObject.SetActive(false);
        }
    }
}