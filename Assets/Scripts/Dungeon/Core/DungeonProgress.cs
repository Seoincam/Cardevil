using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Dungeon
{
    /// <summary>
    /// 던전 진행 상태를 저장하는 데이터 클래스
    /// 세이브/로드 기능에 사용됩니다.
    /// </summary>
    [Serializable]
    public class DungeonProgress
    {
        /// <summary>
        /// 던전 ID
        /// </summary>
        [SerializeField] public int dungeonId;
        
        /// <summary>
        /// 현재 노드 ID
        /// </summary>
        [SerializeField] public int currentNodeId;
        
        /// <summary>
        /// 방문한 노드 ID 목록
        /// </summary>
        [SerializeField] public List<int> visitedNodeIds = new List<int>();
        
        /// <summary>
        /// 지나친 노드 ID 목록 (선택 가능했지만 선택하지 않은 노드들)
        /// </summary>
        [SerializeField] public List<int> passedNodeIds = new List<int>();

        /// <summary>
        /// 던전 완료 여부
        /// </summary>
        [SerializeField] public bool isCompleted;
        
        public DungeonProgress()
        {
            dungeonId = -1;
            currentNodeId = -1;
            visitedNodeIds = new List<int>();
            passedNodeIds = new List<int>();
            isCompleted = false;
        }
        
        public DungeonProgress(int dungeonId, int currentNodeId)
        {
            this.dungeonId = dungeonId;
            this.currentNodeId = currentNodeId;
            visitedNodeIds = new List<int>();
            passedNodeIds = new List<int>();
            isCompleted = false;
        }
        
        /// <summary>
        /// 노드 방문 기록
        /// </summary>
        public void VisitNode(int nodeId)
        {
            if (!visitedNodeIds.Contains(nodeId))
            {
                visitedNodeIds.Add(nodeId);
            }
            currentNodeId = nodeId;
        }
        
        /// <summary>
        /// 노드 방문 여부 확인
        /// </summary>
        public bool HasVisited(int nodeId)
        {
            return visitedNodeIds.Contains(nodeId);
        }
        
        /// <summary>
        /// 노드를 지나침으로 기록 (선택 가능했지만 선택하지 않은 노드)
        /// </summary>
        public void PassNode(int nodeId)
        {
            if (!passedNodeIds.Contains(nodeId))
            {
                passedNodeIds.Add(nodeId);
            }
        }
        
        /// <summary>
        /// 노드 지나침 여부 확인
        /// </summary>
        public bool HasPassed(int nodeId)
        {
            return passedNodeIds.Contains(nodeId);
        }
    }
}

