using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy.Gimmick.Effect
{
    public class Gimmick_Turn_Heal : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;



            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<EnemyTurnEndArgs>.RegisterDynamic(ActionFunction);
        }


        // 턴이 끝난뒤 체력을 회복합니다.
        private void ActionFunction(ExecQueue<EnemyTurnEndArgs> queue, EnemyTurnEndArgs args)
        {
            _targetEnemy.CurrentHp =Mathf.Min( _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]+_targetEnemy.CurrentHp,_targetEnemy.maxHP);
            Debug.Log($"{_targetEnemy.name} : 턴이 종료되어 체력을 { _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]} 만큼 회복하였습니다 ");
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<EnemyTurnEndArgs>.UnregisterDynamic(ActionFunction);
        }
    }
}