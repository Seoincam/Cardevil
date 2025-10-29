using UnityEngine;

namespace Cardevil.Relics
{
    public class DiscardRelic : RelicEffectBase
    {
        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.DiscardCard += 1;
        }
    }
}
