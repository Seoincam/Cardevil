using UnityEngine.UIElements;

namespace Cardevil.Cards.Interactions
{
    public readonly struct CardPointerArgs
    {
        public readonly float time;
        public readonly MouseButton button;

        public CardPointerArgs(float time, MouseButton button)
        {
            this.time = time;
            this.button = button;
        }
    }
}