using System;
using UnityEngine.UIElements;

namespace Cardevil.Cards.InStage
{
    public class CardPointerArgs : EventArgs
    {
        public float Time { get; }

        public CardPointerArgs(float time)
        {
            Time = time;
        }
    }
}