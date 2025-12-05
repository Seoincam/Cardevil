using UnityEngine;

namespace Cardevil.Relics
{
    public class ColorChangeRelicEffect : RelicEffectBase, IRelicEffectOnAcquire
    {
        public void OnAcquire()
        {
            Debug.Log("색깔 변경이 구현되어있지 않습니다");
        }
    }
}
