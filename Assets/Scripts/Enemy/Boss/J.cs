using UnityEngine;
using Cardevil.InGame.Enemy;
using Cardevil.Cards;


namespace Cardevil.InGame.Enemy.Boss
{

    public class J : Enemy
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

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

            // 스트레이트 이상 족보의 공격 받을 시 사용자의 턴 1회 점프
            // 스트레이트 이상 족보라면
            AttackOrderDiscount(); // 공격 턴 1 추가 감소
           


            return false; // 아직 살아있다.
        }
    }

}