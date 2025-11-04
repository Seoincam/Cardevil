using UnityEngine;
using Cardevil.Relics;

namespace Cardevil.Relics
{
    public class HandRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.MaxHand = 7;
        }
    }
}
