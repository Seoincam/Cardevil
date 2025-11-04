using UnityEngine;

namespace Cardevil.Relics
{
    public class DiscardRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.DiscardCard += 1;
        }
    }
}
