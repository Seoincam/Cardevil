using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class DiscardRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Bootstrapper.Instance.Game.PlayerStatus.DiscardCard += 1;
        }
    }
}
