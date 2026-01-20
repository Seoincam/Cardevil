using UnityEngine;
using Cysharp.Threading.Tasks;
using Cardevil.Events.ExecEvents;
using Cardevil.Events;

using System.Threading;
namespace Cardevil.InGame.Enemy
{
    /// <summary>
    /// # 플레이어가 카드를 버릴때마다, 적이 현재 체력의 X%만큼 체력을 회복합니다.
    /// </summary>
    public class Gimmick_Discard_Penalty : IGimmick
    {
        private Enemy _targetEnemy;
        public void Apply(Enemy enemy)
        {
            _targetEnemy = enemy;

            Debug.Log($"{enemy.name} : 랭크 업그레이드 기믹 적용됨");

            // TODO : 기본 카드 버리기 횟수 감소
        }



        // 구독해제
        public void Remove()
        {
            // 필요 없음
        }
    }
}
