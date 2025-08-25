using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy.Boss
{
    public class K : Enemy
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
            if (base.GetDamage(damage))
            {
                return true; // 사망시 스킵
            }

            SetAllAttackOrder(1); // 공격 턴을 1로 만들기

            return false; // 아직 살아있다
        }
    }

}