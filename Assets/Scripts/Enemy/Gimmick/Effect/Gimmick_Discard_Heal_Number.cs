using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 플레이어가 카드를 버릴때마다, 적이 (해당 카드의 숫자) * X%만큼 체력을 회복합니다.
    /// </summary>
    public class Gimmick_Discard_Heal_Number : IGimmick
    {
        private Enemy _targetEnemy;
        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            ExecEventBus<EachCardDiscardedArgs>.RegisterDynamic(CalculateAttackDamage);
        }

        private void CalculateAttackDamage(ExecQueue<EachCardDiscardedArgs> queue, EachCardDiscardedArgs args)
        {
            float newHP;
            newHP = _targetEnemy.HP + (_targetEnemy.HP * (float)args.CardData.NumberSelectState.FinalValue/100);
            _targetEnemy.CurrentHp = Mathf.Min(newHP, _targetEnemy.maxHP);

        }

        // 구독해제
        public void Remove()
        {
            ExecEventBus<EachCardDiscardedArgs>.UnregisterDynamic(CalculateAttackDamage);
        }
    }
}

