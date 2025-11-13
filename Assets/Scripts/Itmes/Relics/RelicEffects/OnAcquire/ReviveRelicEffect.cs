using UnityEngine;

namespace Cardevil.Relics
{
    public class ReviveRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.canRevive = true;
        }
       
    }
}
