using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class RelicData: IDBData {

        /// <summary> 게임상 이름 </summary>
        public string DisplayName;
        /// <summary> 게임상 설명 </summary>
        public string DisplayDescription;
        /// <summary> (Reference:Enum<Cardevil.Relics.RelicRarity>) 
         /// 유물 등급 </summary>
        public string Rarity;
        /// <summary> ID </summary>
        public string RelicId;
        /// <summary> 레벨 </summary>
        public int Level;
        /// <summary> 효과 ID </summary>
        public List<string> EffectIds;
    }
}
