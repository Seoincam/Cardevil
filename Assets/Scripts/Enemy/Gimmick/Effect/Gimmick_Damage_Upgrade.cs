using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;
namespace Cardevil.InGame.Enemy
{
    public class Gimmick_Damage_Upgrade : IGimmick
    {
        private Enemy _targetEnemy;
        private float FirstThresholdRatio = 0.6f;
        private bool _isFirstUpgradeDone = false;
        private ExecAction<EnemyHealthChangeArgs> _handler;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            _isFirstUpgradeDone = false;
            _handler = OnHealthChanged;

            FirstThresholdRatio = enemy.baseMobBossData.GimmickValue[0];
            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<EnemyHealthChangeArgs>.RegisterStatic(0,_handler);
        }
        private async UniTask OnHealthChanged(EnemyHealthChangeArgs args, CancellationToken cancellationToken)
        {
 
            // * Enemy 스크립트에서 args.Init(HP, value, this); 처럼 'this'를 넘겨줘야 함
            if (args.Owner != _targetEnemy)
            {
                return; // 내 적이 아니면 무시
            }

            // 현재 체력 비율 계산 (MaxHp가 Enemy에 있다고 가정)
            float currentRatio = (float)args.ModifiedHealth / _targetEnemy.maxHP;

            // [로직] 트리거 체크 및 랭크 업그레이드

            // 1차 조건: 체력이 X% 미만이고, 아직 1차 업그레이드를 안 했다면
            if (!_isFirstUpgradeDone && currentRatio < FirstThresholdRatio)
            {
                _isFirstUpgradeDone = true;
                UpgradeEnemyDamage();
                Debug.Log($"[Gimmick] {_targetEnemy.name} 체력 {FirstThresholdRatio * 100}% 미만! 데미지 강화!");
            }

       

        }

        /// <summary>
        /// 실제 적의 족보 등급을 올리는 함수
        /// </summary>
        private void UpgradeEnemyDamage()
        {
            // 더 높은 족보 출력
            _targetEnemy.enforcedAttackDamage++;
        }

        public void Remove()
        {
            ExecEventBus<EnemyHealthChangeArgs>.UnregisterStatic(_handler);
        }
    }
}
