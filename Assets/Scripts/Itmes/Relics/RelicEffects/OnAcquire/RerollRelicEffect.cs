using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class RerollRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Bootstrapper.Instance.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
