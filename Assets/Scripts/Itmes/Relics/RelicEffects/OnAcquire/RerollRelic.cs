using UnityEngine;

namespace Cardevil.Relics
{
    public class RerollRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
