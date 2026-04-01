using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Gameplay.Enemy
{
    // TODO : 사용하지않게되면 삭제
    public class EnemyGimmickManager : MonoBehaviour
    {
        // 이 몬스터가 가진 모든 '활성화된' 기믹 로직(POCO)
        private List<IGimmick> activeGimmicks = new List<IGimmick>();

        // 이 매니저가 관리할 대상 (자기 자신)
        private Enemy self;

        void Awake()
        {
            // 자기 자신의 Enemy 컴포넌트를 찾아 캐시
            self = GetComponent<Enemy>();
            if (self == null)
            {
                Debug.LogError("EnemyGimmickManager가 Enemy.cs를 찾을 수 없습니다!");
            }
        }

        // Spawner가 호출할 함수
        public void AddGimmick(IGimmick gimmick)
        {
            if (gimmick == null || self == null) return;

            Debug.Log($"[{self.name}]에게 기믹 {gimmick.GetType().Name} 추가 및 적용.");

            // 1. 리스트에 추가
            activeGimmicks.Add(gimmick);

            // 2. 기믹 적용 (이벤트 구독 등)
            gimmick.Apply(self);
        }

        // (참고) 매 프레임 동작이 필요한 기믹을 위한 처리 (다음 단계에서 설명)
        void Update()
        {
            // 1초마다 독 데미지를 주는 등의 기믹을 위해
            foreach (var gimmick in activeGimmicks)
            {
                // IUpdatableGimmick 같은 별도 인터페이스를 확인
                if (gimmick is IUpdatableGimmick updatableGimmick)
                {
                    updatableGimmick.Tick(Time.deltaTime);
                }
            }
        }
    }
}