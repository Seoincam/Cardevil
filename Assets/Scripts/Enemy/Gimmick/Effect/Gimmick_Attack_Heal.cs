using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;

namespace Cardevil.InGame.Enemy
{
    // # 적이 공격을 성공하면, 최대 체력의 X%만큼을 회복합니다.
    public class Gimmick_Attack_Heal : IGimmick
    {
        private Enemy _targetEnemy;
        private ExecAction<EnemyAttackAfterArgs> _handler;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;
            _handler = EnemyAttack;


            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<EnemyAttackAfterArgs>.RegisterStatic(0,_handler);
        }

        private async UniTask EnemyAttack(EnemyAttackAfterArgs args, CancellationToken cancellationToken)
        {
            if(_targetEnemy._enemyAttackInfo.attackSucess)  // 플레이어 공격에 성공했다면
            {
                // 최대체력의 5% 회복

                _targetEnemy.CurrentHp += _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];
                Debug.Log($"{_targetEnemy.name} : 공격에 성공하여 체력을 { _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]} 만큼 회복하였습니다 ");

            }
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<EnemyAttackAfterArgs>.UnregisterStatic(_handler);
        }


    }
}