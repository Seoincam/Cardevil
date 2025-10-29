using UnityEngine;

namespace Cardevil.Relics
{
    public class ReviveRelic : RelicEffectBase
    {
        /// <summary>
        /// 사용전에 실행되는 ActivateRelicEffect
        /// </summary>
        public override void ActivateRelicEffect()
        {
            Managers.Game.PlayerStatus.canRevive = true;
            // 이친구는 Inventory에서 제거해줘야함.
        }
       
    }
}
