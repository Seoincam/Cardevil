using System.Text;
using System;
using System.Collections.Generic;

namespace Database.Generated
{

    [UnityEngine.Scripting.Preserve]
    [Serializable]
    public partial class BaseMobData: IDBData {

        /// <summary> 몹ID </summary>
        public string MobID;
        /// <summary> 몹이름(한글명) </summary>
        public string MobKorID;
        /// <summary> 몹유형 </summary>
        public string MobType;
        /// <summary> 기본HP </summary>
        public int BaseHP;
        /// <summary> 챕터계수 </summary>
        public float ChapterFactor;
        /// <summary> 성장계수 </summary>
        public int HPGrowFactor;
        /// <summary> 공격주기 </summary>
        public int AttackCycle;
        /// <summary> 공격 데미지 </summary>
        public int AttackDamage;
        /// <summary> 0 - 가중치따라 결정 / 1 - 패턴 고정 </summary>
        public bool BoolAttackType;
        /// <summary> 가중치 </summary>
        public List<int> AttackWeight;
        /// <summary> 사용 족보 순서(오타없이, 띄어쓰기없이) </summary>
        public List<Cardevil.InGame.Enemy.AttackStyle> AttackPattern;
        /// <summary> 0 - 랜덤 / 1 - 유도 </summary>
        public bool AttackPlayer;
        /// <summary> 기믹이름 </summary>
        public List<string> GimmickName;
        /// <summary> 기믹 value </summary>
        public List<float> GimmickValue;
        /// <summary> 1챕터 </summary>
        public string 챕터;
        /// <summary> 17 </summary>
        public string 수;
    }
}
