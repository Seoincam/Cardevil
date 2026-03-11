using Cardevil.Core.Bootstrap;

namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class DiscardRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            // CardevilCore.Game.PlayerStatus.DiscardCard += 1;
        }
    }
}
