using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy.Boss
{
    public class Q : Enemy
    {
        enum State
        {
            Attacked,
            DoNothing
        }

        private State currentState;
        
        public override bool GetDamage(float damage)
        {
            if (base.GetDamage(damage)) // 사망시 true 변환
            {
                return false;
            }
            // 살아 있다면
            // 공격받으면 공격 생성
            SetAttackOnPlayer();
            CreateAttack();
            SetAllAttackOrder(2); // 공격이 터지는 시간을 2로 만든다.


            Debug.Log("피해를 입어 Q 보스의 능력이 발동되었습니다!");

            return false; // 아직 살아있다.
        }

        public override void AttackEnemyAwake() // 처음으로 호출되었을때
        {
            SetFirstAwake(); 
            // 아무것도 안하기
        }
         
    
    }
}
