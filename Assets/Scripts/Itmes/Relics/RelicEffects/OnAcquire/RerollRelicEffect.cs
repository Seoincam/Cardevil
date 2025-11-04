using UnityEngine;

namespace Cardevil.Relics
{
    public class RerollRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
