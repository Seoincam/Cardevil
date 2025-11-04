using UnityEngine;

namespace Cardevil.Relics
{
    public class GoldRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.gold += Random.Range(18,24);
        }
    }
}
