using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy.Gimmick.Effect
{
    /// <summary>
    /// # 턴이 종료될 때, 내 손패에 오망성 카드가 있다면, 적이 최대 체력의 X%만큼 회복합니다.
    /// </summary>
    public class Gimmick_Pentagram_Heal : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;



            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<EnemyTurnEndArgs>.RegisterDynamic(EnemyTurnEndHeal);
        }

        private void EnemyTurnEndHeal(ExecQueue<EnemyTurnEndArgs> queue, EnemyTurnEndArgs args)
        {
            // TODO : 오망성카드를 현재 보유하고 있는지?
            if (_targetEnemy.CurrentHp>=0)  // 체력 테스트, 오망성카드의 존재여부
            {
                // 최대체력의 5% 회복

                _targetEnemy.CurrentHp += _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];
                Debug.Log($"{_targetEnemy.name} : 공격에 성공하여 체력을 { _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]} 만큼 회복하였습니다 ");

            }
        }
        // 구독해제
        public void Remove()
        {
            ExecEventBus<EnemyTurnEndArgs>.UnregisterDynamic(EnemyTurnEndHeal);
        }

    }
}

