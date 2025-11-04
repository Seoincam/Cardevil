using UnityEngine;

namespace Cardevil.Relics
{
    public class MaxHpRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.PlayerStatus.MaxHp += 1;  
        }
    }
}
