using UnityEngine;

namespace Cardevil.Relics
{
    public class AttackDelayRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Managers.Game.Enemy.delayAttackByRelic = 2;
        }
    }
}
