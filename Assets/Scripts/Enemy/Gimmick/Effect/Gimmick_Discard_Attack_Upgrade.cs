using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;      // CancellationToken 사용을 위해 필요

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 플레이어가 버리기를 사용한 횟수만큼, 적의 공격력이 1씩 증가합니다. 공격을 진행하면, 적의 공격력은 1로 초기화됩니다.
    /// </summary>
    public class Gimmick_Discard_Attack_Upgrade : MonoBehaviour
    {
        private Enemy _targetEnemy;
        private ExecAction<CardDiscardChangeArgs> _handler1;
        private ExecAction<PlayerAttackArgs> _handler2;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;


            _handler1 = DamageUpByDiscardCardUsing;
            _handler2 = DamageResetByAttack;
            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<CardDiscardChangeArgs>.RegisterStatic(0, _handler1);
            ExecEventBus<PlayerAttackArgs>.RegisterStatic(0, _handler2);
        }

        private async UniTask DamageUpByDiscardCardUsing(CardDiscardChangeArgs args, CancellationToken cancellationToken)
        {
            // 카드를 버려서 공격력이 증가함
            _targetEnemy.damage++;

        }

        // 공격을 진행해서 데미지가 초기화되었음
        private async UniTask DamageResetByAttack(PlayerAttackArgs args,CancellationToken cancellationToken)
        {
            if (_targetEnemy.damage >= 2) // 매번 발동되지 않게끔 설정
            {
                _targetEnemy.damage = 1;
            }
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<CardDiscardChangeArgs>.UnregisterStatic(_handler1);
        }
    }
}
