using Cardevil.Attributes;
using Cardevil.Core.Turn.Interfaces;
using System;
using UnityEngine;

namespace Cardevil.Core.Turn
{
    /// <summary>
    /// 턴 컨텍스트 읽기 전용 인터페이스.
    /// 현재 턴 상태 및 전투 정보 제공.
    /// </summary>
    public interface IReadOnlyTurnContext
    {
        /// <summary>
        /// 현재 턴의 적 대상.
        /// </summary>
        ITurnTarget CurrentEnemy { get; }
        
        /// <summary>
        /// 플레이어 대상.
        /// </summary>
        ITurnTarget Player { get; }
        
        /// <summary>
        /// 필드 기준 플레이어 위치.
        /// </summary>
        Vector2Int PlayerPosition { get; }
        
        IReadOnlyCardFlow CardFlow { get; }
        
        /// <summary>
        /// 현재 턴 진행 횟수.
        /// </summary>
        int TurnCount { get; }
    }
    
    [Serializable]
    public class TurnContext : IReadOnlyTurnContext
    {
        public ITurnTarget CurrentEnemy { get; set; }
        public ITurnTarget Player { get; set; }
        [field: SerializeField, VisibleOnly] public Vector2Int PlayerPosition { get; set; }
        public IReadOnlyCardFlow CardFlow { get; set; }

        [field: SerializeField, VisibleOnly] public int TurnCount { get; set; }
    }
}