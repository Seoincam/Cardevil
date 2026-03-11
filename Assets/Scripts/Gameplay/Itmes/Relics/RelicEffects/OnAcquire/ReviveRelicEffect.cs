using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class ReviveRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Game.PlayerStatus.canRevive = true;
        }
       
    }
}
