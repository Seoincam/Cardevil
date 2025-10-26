using System;
using UnityEngine.UIElements;

namespace Cardevil.Cards.InStage
{
    public class CardPointerArgs : EventArgs
    {
        public float Time { get; }
        public MouseButton Button { get; } // TODO: 우클릭 감지 필요없음. 없앨 예정.

        public CardPointerArgs(float time, MouseButton button)
        {
            Time = time;
            Button = button;
        }
    }
}