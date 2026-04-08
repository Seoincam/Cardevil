using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class RelicDisplayData: IDBData {

        /// <summary> 식별용 아이디 (구체적인 내용은 적지 않는걸 권장) </summary>
        public string Id;
        /// <summary> 희귀도 (수급 장소) </summary>
        public Cardevil.Gameplay.Relics.Core.RelicRarity Rarity;
        /// <summary> 게임상 이름 </summary>
        public string DisplayName;
        /// <summary> 게임상 설명 </summary>
        public string DisplayDescription;
        /// <summary> 에디터용 코멘트 </summary>
        public string CommentForEditor;
    }
}
