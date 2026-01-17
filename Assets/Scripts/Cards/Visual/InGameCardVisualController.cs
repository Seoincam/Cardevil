using Cardevil.Cards.Data.InStage;
using Cardevil.Cards.Visual.Base;
using Cardevil.Cards.Visual.Controller;
using Cardevil.Cards.Visual.StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Visual
{
    /// <summary>
    /// 애니메이션 없이 즉시 visual을 업데이트하는 컨트롤러.
    /// (주로 인게임에서) 플립 + 업데이트에 쓰임.
    /// </summary>
    public class InGameCardVisualController : CardVisualControllerBase
    {
        private CardVisualBase _visual;

        public override void Init(CardVisualBase visualBase, CardData data)
        {
            _visual = visualBase;
            UpdateVisual(data);
        }

        public void UpdateVisual(CardData data)
        {
            var spriteSet = ConfigureSpriteSet(data);
            switch (spriteSet.Phase)
            {
                case VisualPhase.One: UpdateVisualOne(spriteSet); break;
                case VisualPhase.Two: UpdateVisualTwo(spriteSet); break;
                case VisualPhase.Three: UpdateVisualThree(spriteSet); break;
            }
        }

        
        private void UpdateVisualOne(CardVisualSpriteSet spriteSet)
        {
            // Inactive
            SetInactiveVisualTwo();
            SetInactiveVisualThree();

            // Active
            _visual.InnerFrame.sprite = spriteSet.innerFrame;
            SetSpriteAndActive(_visual.SmallValue, spriteSet.small);
            SetSpriteAndActive(_visual.MainValue, spriteSet.sprites[0]);
        }

        private void UpdateVisualTwo(CardVisualSpriteSet spriteSet)
        {
            // Inactive
            SetInactiveVisualOne();
            SetInactiveVisualThree();

            // Active
            var group = _visual.GetSelectionGroup(VisualPhase.Two);
            group.NumberMap[CardVisualBase.Position.Top].sprite = spriteSet.sprites[0];
            group.NumberMap[CardVisualBase.Position.Bottom].sprite = spriteSet.sprites[1];

            var midSetting = CardVisualBase.BackgroundPos.MidSetting;
            SetPositionAndActive(_visual.SelMidBackground, midSetting.finalPos);
        }

        private void UpdateVisualThree(CardVisualSpriteSet spriteSet)
        {
            // Inactive
            SetInactiveVisualOne();
            SetInactiveVisualTwo();

            // Active
            var group = _visual.GetSelectionGroup(VisualPhase.Three);
            group.NumberMap[CardVisualBase.Position.Top].sprite = spriteSet.sprites[0];
            group.NumberMap[CardVisualBase.Position.Middle].sprite = spriteSet.sprites[1];
            group.NumberMap[CardVisualBase.Position.Bottom].sprite = spriteSet.sprites[2];

            SetPositionAndActive(_visual.SelMidBackground, Vector2.zero);
            SetPositionAndActive(_visual.SelBotBackground, Vector2.zero);
        }

        
        private void SetInactiveVisualOne()
        {
            _visual.MainValue.gameObject.SetActive(false);
            _visual.SmallValue.gameObject.SetActive(false);
        }

        private void SetInactiveVisualTwo()
        {
            var group = _visual.GetSelectionGroup(VisualPhase.Two);
            group.NumberGroup.SetActive(false);

            _visual.SelMidBackground.gameObject.SetActive(false);
            _visual.SelBotBackground.gameObject.SetActive(false);
        }

        private void SetInactiveVisualThree()
        {
            var group = _visual.GetSelectionGroup(VisualPhase.Three);
            group.NumberGroup.SetActive(false);

            _visual.SelMidBackground.gameObject.SetActive(false);
            _visual.SelBotBackground.gameObject.SetActive(false);
        }


        /// <summary>
        /// Sprite를 설정하고 활성화함.
        /// </summary>
        private static void SetSpriteAndActive(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }

        /// <summary>
        /// 위치를 설정하고 활성화함.
        /// </summary>
        private static void SetPositionAndActive(RectTransform rect, Vector2 anchoredPosition)
        {
            rect.anchoredPosition = anchoredPosition;
            rect.gameObject.SetActive(true);
        }
    }
}