using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 암시장과 같은 특별 노드의 생성 및 관리를 담당하는 클래스
    /// </summary>
    [Serializable]
    public class SpecialNodeManager
    {
        // TODO: 암시장 노드 프리셋을 외부에서 주입받도록 설정
        [SerializeField] private DungeonNodeBehaviour blackMarketBehaviour; // 암시장 동작

        /// <summary>
        /// 노드 클리어 시 특별 노드(암시장) 생성 시도
        /// </summary>
        /// <param name="clearedNode">방금 클리어한 노드</param>
        /// <param name="specialNode">생성된 특별 노드 (출력)</param>
        /// <returns>특별 노드 생성 여부</returns>
        public bool TryCreateSpecialNode(DungeonNode clearedNode, out DungeonNode specialNode)
        {
            specialNode = null;
            // TODO: 암시장 등장 확률 로직 구현 (예: 50% 확률)
            if (UnityEngine.Random.value < 0.5f)
            {
                Debug.Log($"[SpecialNodeManager] 암시장 등장! (이전 노드: {clearedNode.NodeId})");

                // 1. 새로운 암시장 노드 생성
                specialNode = new DungeonNode(
                    nodeId: -100, // 임시 특별 ID
                    floor: clearedNode.Floor, // 같은 레벨에 표시
                    type: DungeonNodeTypes.BlackMarket, // 상점 타입
                    behaviour: blackMarketBehaviour
                );
                specialNode.State = NodeState.Available;

                // 2. 암시장 노드 방문 후 돌아갈 다음 노드들 설정
                specialNode.OriginNextNodes = new List<DungeonNode>(clearedNode.NextNodes);

                // 3. 이전 노드의 다음 경로를 암시장으로만 연결
                clearedNode.NextNodes.Clear();
                clearedNode.LinkTo(specialNode);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 특별 노드 상태를 정리하고 원래 흐름으로 복원
        /// </summary>
        /// <param name="specialNode">정리할 특별 노드</param>
        public void ClearSpecialNode(DungeonNode specialNode)
        {
            if (specialNode == null || specialNode.OriginNextNodes == null) return;

            var prevNode = specialNode.PreviousNodes[0]; // 암시장의 이전 노드는 하나뿐이라고 가정
            if (prevNode != null)
            {
                // 이전 노드의 다음 노드 목록을 암시장 방문 전의 상태로 복원
                prevNode.NextNodes.Clear();
                foreach (var originalNextNode in specialNode.OriginNextNodes)
                {
                    prevNode.LinkTo(originalNextNode);
                    // 복원된 노드들을 다시 'Available' 상태로 변경
                    if (originalNextNode.State == NodeState.Locked)
                    {
                        originalNextNode.State = NodeState.Available;
                    }
                }
                LogEx.Log($"특별 노드({specialNode.NodeId}) 정리 완료. 이전 노드({prevNode.NodeId})의 다음 노드들이 복원되었습니다.");
            }
        }
    }
}

