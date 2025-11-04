using UnityEngine;

namespace Cardevil.Relics
{
    public class ReviveRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.canRevive = true;
        }
       
    }
}
