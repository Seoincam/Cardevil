using Cardevil.Core.Bootstrap;

namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class MaxHpRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Game.PlayerStatus.MaxHp += 1;  
        }
    }
}
