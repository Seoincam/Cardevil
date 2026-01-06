using UnityEngine;
using Cardevil.InGame.Enemy;
using Cardevil.Utils; // LogEx 사용 시 필요

namespace Cardevil.InGame.Enemy.Boss
{
    public class A : Enemy
    {
        public override void SetAttack(Attack attack, bool setPlayerAttack = false)
        {
            // 공격 대상 설정
            attack.isPlayerAttack = setPlayerAttack;

            // 로직 클래스에게 시각화 및 데이터 세팅 위임
            HandRankAttackLogic.SetupAttack(attack, this);

            LogEx.Log($"Ace Attack 설정 완료: {attack.currentAttackStyle} (PlayerTarget: {setPlayerAttack})");
        }

        public override void AttackingCheck(Attack attack)
        {
            // 데미지 가져오기
            float damage = baseMobBossData != null ? baseMobBossData.AttackDamage : 1f;

            // 로직 클래스에게 판정 위임
            if (HandRankAttackLogic.CheckHit(attack, damage, out var result))
            {
                // --- 공격 성공 시 ---
                _enemyAttackInfo.attackSucess = true;
                LogEx.Log("Ace 공격 성공!");

                // TODO : 카드를 잠구는 로직
                // Managers.Card.GetCard().Lock(); 
            }
            else
            {
                // --- 공격 실패 시 ---
                _enemyAttackInfo.attackSucess = false;
                LogEx.Log("Ace 공격 실패.");
            }
        }
    }
}