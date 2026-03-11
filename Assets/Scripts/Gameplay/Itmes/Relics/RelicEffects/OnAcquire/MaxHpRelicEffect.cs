using Cardevil.Core.Bootstrap;
using UnityEngine;

namespace Cardevil.Relics
{
    public class MaxHpRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Game.PlayerStatus.MaxHp += 1;  
        }
    }
}
