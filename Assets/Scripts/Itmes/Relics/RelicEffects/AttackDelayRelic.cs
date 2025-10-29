using UnityEngine;

namespace Cardevil.Relics
{
    public class AttackDelayRelic : RelicEffectBase
    {
        public override void ActivateRelicEffect()
        {
            Managers.Game.Enemy.delayAttackByRelic = 2;
        }
    }
}
