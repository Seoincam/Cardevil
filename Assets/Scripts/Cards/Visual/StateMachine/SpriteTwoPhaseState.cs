using Cardevil.Cards.Visual.Base;
using Cardevil.Cards.Visual.Sprites;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteTwoPhaseState : IPhaseState
    {
        private readonly ChangeableCardVisual _visual;
        private ChangeableCardVisual.PhaseGroup _group;
        
        public VisualPhase Kind => VisualPhase.Two;

        private RectTransform Middle => _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Middle];
        private RectTransform Bottom => _visual.SelectionBackgrounds[ChangeableCardVisual.Position.Bottom]; 

        public SpriteTwoPhaseState(ChangeableCardVisual visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardSpriteSet spriteSet)
        {
            _visual.InnerFrame.sprite = spriteSet.InnerFrame;

            _group = _visual.SelectionGroups[VisualPhase.Two];
            _group.Group.SetActive(true);
            
            _group.NumberMap[ChangeableCardVisual.Position.Top].sprite = spriteSet.MainSprites[0];
            _group.NumberMap[ChangeableCardVisual.Position.Bottom].sprite = spriteSet.MainSprites[1];
            
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
            
            _group.Group.SetActive(true);
            // foreach (var image in _group.NumberMap.Values)
            //     image.gameObject.SetActive(false);
        }

        public async UniTask SetPhase(VisualPhase phase)
        {
            switch (phase)
            {
                case VisualPhase.One:
                    await TransitToOneAsync(); break;
                case VisualPhase.Three:
                    await TransitToThreeAsync(); break;
                
                default: return;
            }
        }

        private async UniTask TransitToOneAsync()
        {
            var middleInit = ChangeableCardVisual.BackgroundPositions.MiddleInit;
            var middleFinal =  ChangeableCardVisual.BackgroundPositions.MiddleFinal;
            
            // 초기화
            Middle.anchoredPosition = middleFinal;
            Bottom.gameObject.SetActive(false);
            
            // 트윈
            var dur = .5f;
            await Middle.DOAnchorPos(middleInit, dur);
            Middle.gameObject.SetActive(false);
        }

        private async UniTask TransitToThreeAsync()
        {
            var middleFinal =  ChangeableCardVisual.BackgroundPositions.MiddleFinal;
            var bottomInit =  ChangeableCardVisual.BackgroundPositions.BottomInit;

            
            // 초기화
            Middle.anchoredPosition = middleFinal;

            Bottom.anchoredPosition = bottomInit;
            Bottom.gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(Middle.DOAnchorPos(Vector2.zero, dur))
                .Join(Bottom.DOAnchorPos(Vector2.zero, dur));
        }
    }
}