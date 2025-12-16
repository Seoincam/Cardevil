using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.InGame.Enemy
{


    public class Gimmick_Attack_Upgrade: IGimmick
    {
        private Enemy _targetEnemy;

        int FirstTresholdInt = 0;

        // 상태: 이미 발동했는지 체크하는 플래그
        private bool _isFirstUpgradeDone = false;
   

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            // 초기화
            _isFirstUpgradeDone = false;

           
            ExecEventBus<EnemyHealthChangeArgs>.RegisterDynamic(OnHealthChanged);
        }

        private void OnHealthChanged(ExecQueue<EnemyHealthChangeArgs> queue, EnemyHealthChangeArgs args)
        {
           
            if (args.Owner != _targetEnemy)
            {
                return; // 내 적이 아니면 무시
            }

        }

        /// <summary>
        /// 실제 적의 족보 등급을 올리는 함수
        /// </summary>
        private void UpgradeEnemyRank()
        {
            
        }

        public void OnRemove()
        {
            ExecEventBus<EnemyHealthChangeArgs>.UnregisterDynamic(OnHealthChanged);
        }
    }
}