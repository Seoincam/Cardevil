using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// "# 돌무더기 (플레이어가 해당 칸으로 도착 시, 이동 과정 중 바로 직전에 위치했던 칸으로 되돌리는 기믹)
    ///* 돌무더기 = 내가 해당 장소로 이동하게 되면, 그 장소로 이동한 방향의 역방향으로 1칸 이동시키고 파괴됨. (즉, 스프링처럼 백도 시키는거)"
    /// </summary>
    public class Gimmick_Stone_Trap : IGimmick
    {
        private Enemy _targetEnemy;

        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;



            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");

            // TODO : 돌무더기 설치

        }

        // 구독해제
        public void Remove()
        {
         
        }
    }
}
