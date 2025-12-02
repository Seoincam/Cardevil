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
        /// 마지막 플레이 시간
        /// </summary>
        [SerializeField] public string lastPlayTime;

        /// <summary>
        /// 던전 완료 여부
        /// </summary>
        [SerializeField] public bool isCompleted;
        
        public DungeonProgress()
        {
            dungeonId = -1;
            currentNodeId = -1;
            visitedNodeIds = new List<int>();
            lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            isCompleted = false;
        }
        
        public DungeonProgress(int dungeonId, int currentNodeId)
        {
            this.dungeonId = dungeonId;
            this.currentNodeId = currentNodeId;
            visitedNodeIds = new List<int>();
            lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
            lastPlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        /// <summary>
        /// 노드 방문 여부 확인
        /// </summary>
        public bool HasVisited(int nodeId)
        {
            return visitedNodeIds.Contains(nodeId);
        }
    }
}

