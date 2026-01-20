using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 카드를 여러 장 사용할수록, 완전한 데미지를 가할 수 있습니다. (4장 X% / 3장 X-25% / 2장 X-50% / 1장 X-75%)
    /// </summary>
    public class Gimmick_Cards_Buff : IGimmick
    {
        private Enemy _targetEnemy;
        private ExecAction<PlayerAttackArgs> _handler;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;


            _handler = CalculateCardUsingDamage;
            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<PlayerAttackArgs>.RegisterStatic(0,_handler);
        }

        private async UniTask CalculateCardUsingDamage(PlayerAttackArgs args, CancellationToken cancellationToken)
        {
            // TODO: 구현
            
        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<PlayerAttackArgs>.UnregisterStatic(_handler);
        }
    }
}