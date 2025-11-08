using UnityEngine;

namespace Cardevil.Relics
{
    public class DiscardRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.DiscardCard += 1;
        }
    }
}
