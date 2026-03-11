using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using Cardevil.Utils;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 플레이어가 카드를 버릴때마다, 적이 (해당 카드의 숫자) * X만큼 체력을 회복합니다.
    /// </summary>
    public class Gimmick_Discard_Heal_Number1 : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;



            LogEx.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            // ExecEventBus<CardDiscardChangeArgs>.RegisterDynamic(ActionFunction);
        }


        // // 버린 카드 마다
        // private void ActionFunction(ExecQueue<CardDiscardChangeArgs> queue, CardDiscardChangeArgs args)
        // {
        //     _targetEnemy.CurrentHp = Mathf.Min(_targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0] + _targetEnemy.CurrentHp, _targetEnemy.maxHP);
        //     LogEx.Log($"{_targetEnemy.name} : 카드를 버려 체력을 { _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]} 만큼 회복하였습니다 ");
        // }

        // 구독해제
        public void Remove()
        {
            // ExecEventBus<CardDiscardChangeArgs>.UnregisterDynamic(ActionFunction);
        }
    }
}
