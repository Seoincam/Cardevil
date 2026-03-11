using Cardevil.Core.Events.EventArgs;
using Cardevil.Core.Events.ExecEvent;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy.Gimmick.Effect
{


    public class Gimmick_Rank_Upgrade : IGimmick
    {
        private Enemy _targetEnemy;

        // 설정: 체력 퍼센트 트리거 (예: 60%일 때 1차, 30%일 때 2차)
        private float FirstThresholdRatio = 0.6f;
        private float SecondThresholdRatio = 0.3f;

        // 상태: 이미 발동했는지 체크하는 플래그
        private bool _isFirstUpgradeDone = false;
        private bool _isSecondUpgradeDone = false;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            // 초기화
            _isFirstUpgradeDone = false;
            _isSecondUpgradeDone = false;

            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");
            FirstThresholdRatio = enemy.baseMobBossData.GimmickValue[0];
            if(enemy.baseMobBossData.GimmickValue.Count>=2)
            {
                SecondThresholdRatio = enemy.baseMobBossData.GimmickValue[1];
            }

            ExecEventBus<EnemyHealthChangeArgs>.RegisterDynamic(OnHealthChanged);
        }

        private void OnHealthChanged(ExecQueue<EnemyHealthChangeArgs> queue,EnemyHealthChangeArgs args)
        {
            Debug.Log("작동?!");
            // [중요 1] 이 이벤트가 '내 적'에게서 발생한 건지 확인
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
                UpgradeEnemyRank();
                Debug.Log($"[Gimmick] {_targetEnemy.name} 체력 {FirstThresholdRatio * 100}% 미만! 1차 족보 강화!");
            }

            // 2차 조건: 체력이 Y% 미만이고, 아직 2차 업그레이드를 안 했다면
            if (!_isSecondUpgradeDone && currentRatio < SecondThresholdRatio)
            {
                _isSecondUpgradeDone = true;
                UpgradeEnemyRank();
                Debug.Log($"[Gimmick] {_targetEnemy.name} 체력 {SecondThresholdRatio * 100}% 미만! 2차 족보 강화!");
            }

        }

        /// <summary>
        /// 실제 적의 족보 등급을 올리는 함수
        /// </summary>
        private void UpgradeEnemyRank()
        {
            // 더 높은 족보 출력
            _targetEnemy.enforcedAttackRanking++;
        }

        public void Remove()
        {
            ExecEventBus<EnemyHealthChangeArgs>.UnregisterDynamic(OnHealthChanged);
        }
    }
}