using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.InGame.Enemy
{
    public class Gimmick_Test : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            ExecEventBus<EnemyHealthChangeArgs>.RegisterDynamic(ActionFunction);
        }

        private void ActionFunction(ExecQueue<EnemyHealthChangeArgs> queue, EnemyHealthChangeArgs args)
        {

        }

        // 구독해제
        public void Remove()
        {

        }

    }
}

/*
 * 
 * 이런식으로 등록
 *public float CurrentHp
{
    get => HP;
    set
    {
        using (EnemyHealthChangeArgs args = EnemyHealthChangeArgs.Get())
        {
            args.Init(HP, value,this);
            ExecEventBus<EnemyHealthChangeArgs>.InvokeMerged(args).Forget();
            HP = args.ModifiedHealth;
        }
    }
}
*/