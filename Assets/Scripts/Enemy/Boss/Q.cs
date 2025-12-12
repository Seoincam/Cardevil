using Cardevil.Cards.Data;
using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy.Boss
{
    public class Q : Enemy
    {
        enum State
        {
            Normal,
            Engage
        }
        public int EngageTurn = 0;


        private State currentState;

     
        public override bool GetDamage(float damage)
        {
            if (base.GetDamage(damage)) // 사망시 true 변환
            {
                return true;
            }

            // 살아 있다면
            // 공격받으면 공격 생성
            // 스트레이트 이상 족보의 공격 받을 시
            /*
            if (Managers.Card.EvaluationResults.CurrentResult.HandRanking >= HandRanking.Straight)
            {
                Debug.Log("Q은 Straight이상의 공격을 받았습니다.");

                if(currentState ==State.Engage)
                {
                    //이미 분노 상태였다면
                    SetEngageState();

                    return false;
                }
                Debug.Log("피해를 입어 Q 보스의 능력이 발동되었습니다!");
                SetAttackOnPlayer();
                CreateAttack();
                SetAllAttackOrder(2); // 공격이 터지는 시간을 2로 만든다.
            }
            EngageTurn--;
            if(EngageTurn==0)
            {
                SetNormalState();
            }
            */
           
        
            

            return false; // 아직 살아있다.
        }

        public override void AttackEnemyAwake() // 처음으로 호출되었을때
        {
            SetFirstAwake();
            currentState = State.Normal;
            // 아무것도 안하기
        }
         
        private void SetEngageState()
        {
            EngageTurn = 2;
            currentState = State.Engage;
        }
        private void SetNormalState()
        {
            currentState = State.Normal;
        }
    
    }
}
