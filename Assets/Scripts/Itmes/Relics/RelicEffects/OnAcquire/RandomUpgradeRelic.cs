using UnityEngine;

namespace Cardevil.Relics
{
    public class RandomUpgradeRelic : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Debug.Log("랜덤 강화가 구현되어있지 않습니다");
        }
    }
}
