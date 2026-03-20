using System;
using System.Collections.Generic;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class RelicSaveData
    {
        public string relicId;
        public List<EffectSaveData> effectStates = new();
    }
    
    [Serializable]
    public class EffectSaveData
    {
        /// <summary>
        /// Definition.Effects 리스트의 몇 번째 이펙트인지 식별.
        /// </summary>
        public int effectIndex;
        
        /// <summary>
        /// 상태 객체를 직렬화한 데이터.
        /// </summary>
        public string saveData;
    }
}