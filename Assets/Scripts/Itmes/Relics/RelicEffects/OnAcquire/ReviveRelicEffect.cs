using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class ReviveRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Bootstrapper.Instance.Game.PlayerStatus.canRevive = true;
        }
       
    }
}
