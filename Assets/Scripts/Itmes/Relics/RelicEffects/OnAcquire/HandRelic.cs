using UnityEngine;
using Cardevil.Relics;

namespace Cardevil.Relics
{
    public class HandRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.MaxHand = 7;
        }
    }
}
