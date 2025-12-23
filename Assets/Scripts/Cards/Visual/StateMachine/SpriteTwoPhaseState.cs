using Cardevil.Cards.Visual.Base;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public class SpriteTwoPhaseState : IPhaseState
    {
        private readonly CardVisualBase _visual;
        private CardVisualBase.SelectionGroup _group;
        
        public VisualPhase Kind => VisualPhase.SpriteTwo;

        public SpriteTwoPhaseState(CardVisualBase visual)
        {
            _visual = visual;
        }
        
        // 숫자가 크기가 커지며 나타남
        public async UniTask OnEnter(CardVisualSpriteSet spriteSet)
        {
            _visual.InnerFrame.sprite = spriteSet.innerFrame;
            
            _group ??= _visual.GetSelectionGroup(Kind);
            _group.NumberGroup.SetActive(true);
            
            _group.NumberMap[CardVisualBase.Position.Top].sprite = spriteSet.sprites[0];
            _group.NumberMap[CardVisualBase.Position.Bottom].sprite = spriteSet.sprites[1];
            
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
            var midSetting = CardVisualBase.BackgroundPos.MidSetting;
            
            // 초기화
            _visual.SelMidBackground.anchoredPosition = midSetting.finalPos;
            
            _visual.SelBotBackground.gameObject.SetActive(false);
            
            // 트윈
            var dur = .5f;
            await _visual.SelMidBackground.DOAnchorPos(midSetting.initPos, dur);
            _visual.SelMidBackground.gameObject.SetActive(false);
        }

        private async UniTask TransitToThreeAsync()
        {
            var midSetting = CardVisualBase.BackgroundPos.MidSetting;
            var botSetting = CardVisualBase.BackgroundPos.BotSetting;
            
            // 초기화
            _visual.SelMidBackground.anchoredPosition = midSetting.finalPos;

            _visual.SelBotBackground.anchoredPosition = botSetting.initPos;
            _visual.SelBotBackground.gameObject.SetActive(true);
            
            // 트윈
            var dur = .5f;
            await DOTween.Sequence()
                .Join(_visual.SelMidBackground.DOAnchorPos(Vector2.zero, dur))
                .Join(_visual.SelBotBackground.DOAnchorPos(Vector2.zero, dur));
        }
    }
}