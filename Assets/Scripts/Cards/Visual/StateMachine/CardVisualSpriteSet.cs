using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public readonly struct CardVisualSpriteSet
    {
        public readonly Sprite innerFrame;
        public readonly List<Sprite> sprites;
        public readonly Sprite small;

        public CardVisualSpriteSet(Sprite innerFrame, List<Sprite> sprites, Sprite small = null)
        {
            this.innerFrame = innerFrame;
            this.sprites = sprites;
            this.small = small;
        }
    }
}