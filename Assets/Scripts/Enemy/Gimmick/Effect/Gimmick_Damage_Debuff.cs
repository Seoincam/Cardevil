using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 적이 공격을 성공하면, X턴간 내가 입히는 데미지가 Y%로 감소합니다.
    /// </summary>
    public class Gimmick_Damage_Debuff : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            ExecEventBus<PlayerAttackArgs>.RegisterDynamic(CalculateDamage);
        }

        // 플레이어가 공격할때 데미지 계산 
        private void CalculateDamage(ExecQueue<PlayerAttackArgs> queue, PlayerAttackArgs args)
        {
            // 공격에 성공했다면
            if(_targetEnemy._enemyAttackInfo.attackSucess)
            {
                float _damageAmount = args.DamageAmount;
                _damageAmount = _damageAmount * _targetEnemy.baseMobBossData.GimmickValue[0];
                args.SetValues(_damageAmount);
            }
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<PlayerAttackArgs>.UnregisterDynamic(CalculateDamage);
        }

    }
}
