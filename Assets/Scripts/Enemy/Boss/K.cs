using UnityEngine;
using Cardevil.InGame.Enemy;

namespace Cardevil.InGame.Enemy.Boss
{
    public class K : Enemy
    {
        // 항상 유도공격만함

        public override bool GetDamage(float damage)
        {
            if (base.GetDamage(damage))
            {
                return true; // 사망시 스킵
            }

            return false; // 아직 살아있다
        }

        public override void SetAttack(Attack attack, bool setPlayerAttack = false)
        {
            if (setPlayerAttack) // 플레이어 위치로 공격할 것인가에 대해
            {
                SetPlayerAttack(attack);
                Debug.Log($"KingAttack 예상 sign {attack.currentAttackStyle}");
            }

        }

        public override void AttackingCheck(Attack attack)
        {
            Debug.Log("AttackingCheck");
            if(AttackGo(attack))
            {
                // 공격에 성공했음
                Debug.Log("King이 공격에 성공했다!");
                SetAllAttackOrder(1);
            }
            else
            {
                Debug.Log("King이 공격에 실패했다!");
                //공격에 실패했음.
            }
        }
    }

}