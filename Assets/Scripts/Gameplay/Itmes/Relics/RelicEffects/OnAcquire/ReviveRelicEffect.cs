using Cardevil.Core.Bootstrap;

namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class ReviveRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            // CardevilCore.Game.PlayerStatus.canRevive = true;
        }
       
    }
}
