using Cardevil.Core.Bootstrap;

namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class HandRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            // CardevilCore.Game.PlayerStatus.MaxHand = 7;
        }
    }
}
