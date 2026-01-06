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



            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");


            ExecEventBus<EnemyHealthChangeArgs>.RegisterDynamic(ActionFunction);
        }

        private void ActionFunction(ExecQueue<EnemyHealthChangeArgs> queue, EnemyHealthChangeArgs args)
        {

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
    }
}