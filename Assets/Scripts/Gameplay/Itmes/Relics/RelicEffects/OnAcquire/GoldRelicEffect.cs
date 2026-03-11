namespace Cardevil.Gameplay.Items.RelicItem.RelicEffects.OnAcquire
{
    public class GoldRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            // Bootstrapper.Instance.Game.PlayerStatus.gold += Random.Range(18,24);
        }
    }
}
