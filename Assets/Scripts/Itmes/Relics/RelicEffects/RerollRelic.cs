using UnityEngine;

namespace Cardevil.Relics
{
    public class RerollRelic : RelicEffectBase
    {
        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
