using Cardevil.Cards.Visual.Base;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteOnePhaseState : IPhaseState
    {
        private readonly CardVisualBase _visual;
        public VisualPhase Kind => VisualPhase.SpriteOne;

        public SpriteOnePhaseState(CardVisualBase visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardVisualSpriteSet spriteSet)
        {
            _visual.SmallValue.gameObject.SetActive(false);
            
            _visual.InnerFrame.sprite = spriteSet.innerFrame;
            _visual.MainValue.sprite = spriteSet.sprites[0];
            _visual.MainValue.gameObject.SetActive(true);
            
            var seq = DOTween.Sequence()
                .Join(_visual.MainValue.rectTransform.DOScale(1f, .5f));

            if (spriteSet.small)
            {
                _visual.SmallValue.sprite = spriteSet.small;
                _visual.SmallValue.gameObject.SetActive(true);
                seq.Join(_visual.SmallValue.DOFade(1f, .5f));
            }

            await seq;
        }

        // 숫자가 크기가 작아지며 사라짐
        public async UniTask OnExit()
        {
            var seq = DOTween.Sequence()
                .Join(_visual.MainValue.rectTransform.DOScale(0f, .5f));
            if (_visual.SmallValue.gameObject.activeSelf)
                seq.Join(_visual.SmallValue.DOFade(0f, .5f));

            await seq;

            _visual.MainValue.gameObject.SetActive(false);
            _visual.SmallValue.gameObject.SetActive(false);
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
            var midSetting = CardVisualBase.BackgroundPos.MidSetting;

            // 초기화
            _visual.SelBotBackground.gameObject.SetActive(false);

            _visual.SelMidBackground.anchoredPosition = midSetting.initPos;
            _visual.SelMidBackground.gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await _visual.SelMidBackground.DOAnchorPos(midSetting.finalPos, dur);
        }

        private async UniTask TransitToThreeAsync()
        {
            var midSetting = CardVisualBase.BackgroundPos.MidSetting;
            var botSetting = CardVisualBase.BackgroundPos.BotSetting;
            
            // 초기화
            _visual.SelBotBackground.anchoredPosition = botSetting.initPos;
            _visual.SelBotBackground.gameObject.SetActive(true);

            _visual.SelMidBackground.anchoredPosition = midSetting.initPos;
            _visual.SelMidBackground.gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(_visual.SelBotBackground.DOAnchorPos(Vector2.zero, dur))
                .Join(_visual.SelMidBackground.DOAnchorPos(Vector2.zero, dur));
        }
    }
}