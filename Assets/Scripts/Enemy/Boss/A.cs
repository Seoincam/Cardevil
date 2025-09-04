using UnityEngine;
using Cardevil.InGame.Enemy;


namespace Cardevil.InGame.Enemy.Boss
{
    public class A : Enemy
    {
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
            AttackGo(attack);
            Managers.Card.GetCard().Lock(); // 카드 잠구기
        }

    }

}