using UnityEngine;
using Cardevil.Relics;

namespace Cardevil.Relics
{
    public class HandRelic : RelicEffectBase
    {
        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.MaxHand = 7;
        }
    }
}
