using UnityEngine;

namespace Cardevil.Relics
{
    public class GoldRelic : RelicEffectBase
    {

        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.gold += Random.Range(18,24);
        }
    }
}
