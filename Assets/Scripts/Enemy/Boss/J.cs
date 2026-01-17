using UnityEngine;
using Cardevil.InGame.Enemy;


namespace Cardevil.InGame.Enemy.Boss
{

    public class J : Enemy
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        // Update is called once per frame
        void Update()
        {

        }

        public override bool GetDamage(float damage)
        {
            if (base.GetDamage(damage)) // 사망시 true 변환
            {
                return false;
            }
            // 살아 있다면

            /*
            var handRanking = Managers.Card.EvaluationResults.CurrentResult.HandRanking;
            Debug.Log($"Jack이 생존하여 {handRanking} 에 대한 반격을 진행한다");

            // 스트레이트 이상 족보의 공격 받을 시 사용자의 턴 1회 점프
            if (handRanking >= HandRanking.Straight)
            {
                Debug.Log("Jack은 Straight이상의 공격을 받았습니다.");
                AttackOrderDiscount(); // 공격 턴 1 추가 감소
            }
           
           */


            return false; // 아직 살아있다.
        }
    }

}