using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;
using System.Threading;

namespace Cardevil.InGame.Enemy
{

    /// <summary>
    /// # 플레이어의 남은 카드 장 수가 X장 미만이면, 적의 공격력이 Y로 증가합니다.
    /// </summary>
    public class Gimmick_Attack_Upgrade: IGimmick
    {
        private Enemy _targetEnemy;

        int FirstTresholdInt = 0;

        // 상태: 이미 발동했는지 체크하는 플래그
        private bool _isFirstUpgradeDone = false;
        private ExecAction<EachCardDiscardedArgs> _handler;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            // 초기화
            _isFirstUpgradeDone = false;
            _handler = FunctionAction;

           
            ExecEventBus<EachCardDiscardedArgs>.RegisterStatic(0,_handler);
        }

        private async UniTask FunctionAction(EachCardDiscardedArgs args, CancellationToken cancellationToken)
        {
            



        }

        /// <summary>
        /// 실제 적의 족보 등급을 올리는 함수
        /// </summary>
        private void UpgradeEnemyRank()
        {
            
        }

        public void Remove()
        {
            ExecEventBus<EachCardDiscardedArgs>.UnregisterStatic(_handler);
        }
    }
}