using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;      // CancellationToken 사용을 위해 필요

/// <summary>
/// # 최대 체력의 X% 이상의 피해를 받으면, 초과된 피해량만큼을 0으로 만듭니다.
/// </summary>

namespace Cardevil.InGame.Enemy
{
    public class Gimmick_Damage_Limit : IGimmick
    {
        private Enemy _targetEnemy;

        // 핸들러를 멤버 변수로 캐싱 
        // private ExecAction<CardDamageEvaluationArgs> _handler;


        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;
            // _handler = CalculateAttackDamage;
            //
            // // TODO: 대윤 - 우선순위 체크해야함.
            // int priority = (int)CardDamageEvaluationArgs.Orders.MultiplyPlayerStatus + 10;
            // ExecEventBus<CardDamageEvaluationArgs>.RegisterStatic(priority, _handler);
        }

        // private async UniTask CalculateAttackDamage(CardDamageEvaluationArgs args, CancellationToken cancellationToken)
        // {
        //     // 제한할 데미지 상한선 계산 (최대 체력 * 기믹 설정값 %)
        //     float damageCap = _targetEnemy.maxHP * _targetEnemy.baseMobBossData.GimmickValue[0];
        //
        //     // 들어온 데미지가 상한선을 초과하는지 확인
        //     if (args.Damage > damageCap)
        //     {
        //         var originalDamage = args.Damage;
        //
        //         // 데미지 고정
        //         args.ClampDamage(damageCap);
        //
        //         Debug.Log($"[Gimmick] 데미지 제한 발동: {originalDamage} -> {args.Damage} (최대치: {damageCap})");
        //     }
        // }

        public void Remove()
        {

        }

    }
}
