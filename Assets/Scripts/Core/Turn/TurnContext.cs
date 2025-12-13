using Cardevil.Attributes;
using Cardevil.Core.Turn.Interfaces;
using System;
using UnityEngine;

namespace Cardevil.Core.Turn
{
    public interface IReadOnlyTurnContext
    {
        ITurnTarget CurrentEnemy { get; }
        ITurnTarget Player { get; }
        Vector2Int PlayerPosition { get; }
        IReadOnlyCardFlow CardFlow { get; }
        
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