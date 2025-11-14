using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Cards.Visual.StateMachine
{
    public readonly struct CardVisualSpriteSet
    {
        public readonly Sprite innerFrame;
        public readonly List<Sprite> sprites;

        public CardVisualSpriteSet(Sprite innerFrame, List<Sprite> sprites)
        {
            this.innerFrame = innerFrame;
            this.sprites = sprites;
        }
    }
}