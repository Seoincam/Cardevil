using Cardevil.Attributes;
using Cardevil.Items.Relics.Factory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Relics
{
    [Serializable]
    public class RelicManager
    {
        [SerializeField, VisibleOnly] private List<Relic> relics;

        public void Init()
        {
            RelicFactory.RegisterAllFactory();
            Managers.Database.OnInitialized += MakeInstances;
        }
        
        /// <summary>
        /// Data를 기반으로 실제 Relic 및 RelicEffect를 생성.
        /// </summary>
        private void MakeInstances()
        {
            relics = RelicFactory.MakeRelicInstances();
        }
        
        /// <summary>
        /// 유물을 획득하고 PlayerStatus로 넘겨줍니다.
        /// </summary>
        public void GetRelicToPlayer(RelicEffectBase relicEffectBase)
        {
            Managers.Game.PlayerStatus.relicEffectBases.Add(relicEffectBase);
        }
        
        /// <summary>
        /// 게임이 시작할때 유물의 효과를 세팅합니다.
        /// </summary>
        public void SettingRelicsEffectBase()
        {
            List<RelicEffectBase> relicList = Managers.Game.PlayerStatus.relicEffectBases; 
            foreach(var r in relicList)
            {
                r.ActivateRelicEffect();
            }
        }
    }
}
