using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class RerollRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Instance.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
