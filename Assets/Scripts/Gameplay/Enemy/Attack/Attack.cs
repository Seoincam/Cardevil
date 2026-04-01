using Cardevil.Core.Utils;

namespace Cardevil.Gameplay.Enemy.Attack
{
    // Attack.cs 로 분리 추천
    public class Attack
    {
        public AttackStyle currentAttackStyle;
        public int attackLineNumber;
        public int attackCycle = 3;
        public int attackTurnOrder;
        public int damage = 1;

        // 좌표 데이터
        public int attackPointNumber_x = 0;
        public int attackPointNumber_y = 0;
        public int[] attackPointNumberExtra_x;
        public int[] attackPointNumberExtra_y;

        // 상태 데이터
        public bool isPlayerAttack = true;
        public TileVector playerPosition;
        public int excludedCornerIndex = -1;

        public void SetAttackCycle(int cycle)
        {
            attackCycle = cycle;
            attackTurnOrder = attackCycle;
        }
    }
}