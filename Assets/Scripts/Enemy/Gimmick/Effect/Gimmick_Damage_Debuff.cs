using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 적이 받는 데미지가 최대 체력의 X%를 초과하지 않도록 제한합니다.
    /// </summary>
    public class Gimmick_Damage_Debuff : IGimmick
    {
        private Enemy _targetEnemy;
        private ExecAction<PlayerAttackArgs> _handler;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;
            _handler = CalculateAttackDamage;

            ExecEventBus<PlayerAttackArgs>.RegisterStatic((int)PlayerAttackArgs.Order.Phase_Final, _handler);
        }

        private async UniTask CalculateAttackDamage(PlayerAttackArgs args, CancellationToken cancellationToken)
        {
            // 제한할 데미지 상한선 계산 (최대 체력 * 기믹 설정값 %)
            float damageCap = _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];

            // 들어온 데미지가 상한선을 초과하는지 확인
            if (args.DamageAmount > damageCap)
            {
                // 로그
                float originalDamage = args.DamageAmount;

                // 데미지 고정
                args.SetValues(damageCap);

                Debug.Log($"[Gimmick] 데미지 제한 발동: {originalDamage} -> {args.DamageAmount} (최대치: {damageCap})");
            }
        }

        public void Remove()
        {
            if (_handler != null)
            {
                ExecEventBus<PlayerAttackArgs>.UnregisterStatic(_handler);
                _handler = null;
            }
        }

    }
}