using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class GoldRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Bootstrapper.Instance.Game.PlayerStatus.gold += Random.Range(18,24);
        }
    }
}
