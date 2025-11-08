using UnityEngine;

namespace Cardevil.Relics
{
    public class GoldRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.gold += Random.Range(18,24);
        }
    }
}
