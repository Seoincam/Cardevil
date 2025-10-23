using Cardevil.Attributes;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.Cards.Data.InStage
{
    [Serializable]
    public class CardVisualSpriteSet
    {
        // 아직 카드 그래픽이 확정 안 나서
        // 이 클래스는 계속 바뀔 수 있음
        
        [SerializeField, VisibleOnly] private Sprite frontBackgroundImage;
        [SerializeField, VisibleOnly] private Sprite frontNumberImage;
        
        public Sprite FrontBackgroundImage => frontBackgroundImage;
        public Sprite FrontNumberImage => frontNumberImage;

        public CardVisualSpriteSet(Sprite frontBackgroundImage, Sprite frontNumberImage)
        {
            this.frontBackgroundImage = frontBackgroundImage;
            this.frontNumberImage = frontNumberImage;
        }
    }
}