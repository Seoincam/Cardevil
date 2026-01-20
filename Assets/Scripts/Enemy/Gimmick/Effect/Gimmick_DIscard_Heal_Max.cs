using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using Cardevil.Utils;
using System.Threading;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 플레이어가 카드를 버릴때마다, 적이 현재 체력의 X%만큼 체력을 회복합니다.
    /// </summary>
    public class Gimmick_Discard_Heal_Max : IGimmick
    {
        private Enemy _targetEnemy;
        

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;



            LogEx.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<CardDiscardChangeArgs>.RegisterStatic(0, ActionFunction);
        }


        // 카드 버리기 버튼을 누를때
        private async UniTask ActionFunction(CardDiscardChangeArgs args, CancellationToken cancellationToken)
        {
            _targetEnemy.CurrentHp = Mathf.Min(_targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0] + _targetEnemy.CurrentHp, _targetEnemy.maxHP);
            LogEx.Log($"{_targetEnemy.name} : 카드를 버려 체력을 { _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0]} 만큼 회복하였습니다 ");
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<CardDiscardChangeArgs>.UnregisterStatic(ActionFunction);
        }
    }
}
