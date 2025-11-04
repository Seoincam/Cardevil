using UnityEngine;

namespace Cardevil.Relics
{
    public class AttackDelayRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.Enemy.delayAttackByRelic = 2;
        }
    }
}
