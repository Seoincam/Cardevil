using UnityEngine;
using Cardevil.InGame.Enemy;
using Cardevil.Utils; 

namespace Cardevil.InGame.Enemy.Boss
{
    public class K : Enemy
    {
        // K(King) 특징: 항상 유도 공격(PlayerAttack)만 수행함

        public override bool GetDamage(float damage)
        {
            if (base.GetDamage(damage))
            {
                return true; // 사망 시 처리
            }
            return false; // 생존
        }

        // 공격 설정 (Setup)
        public override void SetAttack(Attack attack, bool setPlayerAttack = false)
        {
            // 플레이어 조준
            attack.isPlayerAttack = true;

            HandRankAttackLogic.SetupAttack(attack, this);

            LogEx.Log($"KingAttack 설정 완료: {attack.currentAttackStyle} (유도)");
        }

        // 공격 판정 (Check)
        public override void AttackingCheck(Attack attack)
        {

            float damage = baseMobBossData.AttackDamage;

        
            if (HandRankAttackLogic.CheckHit(attack, damage, out var result))
            {
                // 공격 성공
                LogEx.Log("King이 공격에 성공했다!");
                _enemyAttackInfo.attackSucess = true;


                // King만의 특수 로직 
                SetAllAttackOrder(1);
            }
            else
            {
                // 공격 실패
                _enemyAttackInfo.attackSucess = false;
                LogEx.Log("King이 공격에 실패했다!");
            }
        }
    }
}