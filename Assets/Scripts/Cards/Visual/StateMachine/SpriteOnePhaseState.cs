using Cardevil.Cards.Visual.Base;
using Cardevil.Cards.Visual.Sprites;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteOnePhaseState : IPhaseState
    {
        private readonly ChangeableCardVisual _visual;
        public VisualPhase Kind => VisualPhase.One;

        public SpriteOnePhaseState(ChangeableCardVisual visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardSpriteSet spriteSet)
        {
            _visual.SmallNumber.gameObject.SetActive(false);
            
            _visual.InnerFrame.sprite = spriteSet.InnerFrame;
            _visual.PrimaryValue.sprite = spriteSet.Primary;
            _visual.PrimaryValue.gameObject.SetActive(true);
            
            var seq = DOTween.Sequence()
                .Join(_visual.PrimaryValue.rectTransform.DOScale(1f, .5f));

            if (spriteSet.HasSmallNumber)
            {
                _visual.SmallNumber.sprite = spriteSet.SmallNumber;
                _visual.SmallNumber.gameObject.SetActive(true);
                seq.Join(_visual.SmallNumber.DOFade(1f, .5f));
            }

            await seq;
        }

        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            var seq = DOTween.Sequence()
                .Join(_visual.PrimaryValue.rectTransform.DOScale(0f, .5f));
            if (_visual.SmallNumber.gameObject.activeSelf)
                seq.Join(_visual.SmallNumber.DOFade(0f, .5f));

            await seq;

            _visual.PrimaryValue.gameObject.SetActive(false);
            _visual.SmallNumber.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.Two:
                    await TransitToTwoAsync(); break;
                case VisualPhase.Three:
                    await TransitToThreeAsync(); break;
                
                default: return;
            } 
        }

        private async UniTask TransitToTwoAsync()
        {
            var middleInit = ChangeableCardVisual.BackgroundPositions.MiddleInit;
            var middleFinal = ChangeableCardVisual.BackgroundPositions.MiddleFinal;

            // 초기화
            _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Bottom].gameObject.SetActive(false);

            _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Middle].anchoredPosition = middleInit;
            _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Middle].gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Middle].DOAnchorPos(middleFinal, dur);
        }

        private async UniTask TransitToThreeAsync()
        {
            var middleInit = ChangeableCardVisual.BackgroundPositions.MiddleInit;
            var bottomInit = ChangeableCardVisual.BackgroundPositions.BottomInit;
            
            // 초기화
            var bottom = _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Bottom];
            var middle = _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Bottom];
            
            bottom.anchoredPosition = bottomInit;
            bottom.gameObject.SetActive(true);

            middle.anchoredPosition = middleInit;
            middle.gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(bottom.DOAnchorPos(Vector2.zero, dur))
                .Join(middle.DOAnchorPos(Vector2.zero, dur));
        }
    }
}