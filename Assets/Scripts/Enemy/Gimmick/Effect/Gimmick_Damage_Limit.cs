using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

/// <summary>
/// # 최대 체력의 X% 이상의 피해를 받으면, 초과된 피해량만큼을 0으로 만듭니다.
/// </summary>

namespace Cardevil.InGame.Enemy
{
    public class Gimmick_Damage_Limit : IGimmick
    {
        private Enemy _targetEnemy;
        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            ExecEventBus<PlayerAttackArgs>.RegisterDynamic(CalculateAttackDamage);


            
        }

        private void CalculateAttackDamage(ExecQueue<PlayerAttackArgs> queue, PlayerAttackArgs args)
        {
            // 1. 제한할 데미지 상한선 계산 (최대 체력 * 기믹 설정값 %)
            // 예: GimmickValue[0]이 0.1이면 10%
            float damageCap = _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];

            // 2. 들어온 데미지가 상한선을 초과하는지 확인
            if (args.DamageAmount > damageCap)
            {
                // 로그
                float originalDamage = args.DamageAmount;
                    
                // 데미지 고정
                args.SetValues(damageCap);

                Debug.Log($"[Gimmick] 데미지 제한 발동: {originalDamage} -> {args.DamageAmount} (최대치: {damageCap})");
            }
        }
    }
    
}
