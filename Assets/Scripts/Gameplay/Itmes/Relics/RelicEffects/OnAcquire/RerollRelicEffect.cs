using Cardevil.Core.Bootstrap;

namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class RerollRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            CardevilCore.Game.PlayerStatus.RerollTicket += 3;
        }
    }
}
