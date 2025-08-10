using UnityEngine;
using Cardevil.InGame.Enemy;


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
            if (base.GetDamage(damage)) // 기본 데미지를 받고 죽지않았다면
            {
                return false;
            }

            // 스트레이트 이상 족보의 공격 받을 시 사용자의 턴 1회 점프


            return false; // 아직 살아있다.
        }
    }

}