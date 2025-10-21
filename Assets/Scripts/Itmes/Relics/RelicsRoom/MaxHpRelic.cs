using UnityEngine;

namespace Cardevil.Relics
{
    public class MaxHpRelic : RelicEffectBase
    {
        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.MaxHp += 1;  
        }
    }
}
