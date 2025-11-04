using UnityEngine;

namespace Cardevil.Relics
{
    public class MaxHpRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.MaxHp += 1;  
        }
    }
}
