using Cardevil.Core.Bootstrap;
using UnityEngine;
using Cardevil.Relics;

namespace Cardevil.Relics
{
    public class HandRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Instance.Game.PlayerStatus.MaxHand = 7;
        }
    }
}
