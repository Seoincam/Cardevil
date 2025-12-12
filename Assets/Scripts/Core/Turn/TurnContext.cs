using Cardevil.Core.Turn.Interfaces;
using UnityEngine;

namespace Cardevil.Core.Turn
{
    public interface IReadOnlyTurnContext
    {
        ITurnTarget CurrentEnemy { get; }
        ITurnTarget Player { get; }
        Vector2Int PlayerPosition { get; }
    }
    
    public class TurnContext : IReadOnlyTurnContext
    {
        public ITurnTarget CurrentEnemy { get; set; }
        public ITurnTarget Player { get; set; }
        public Vector2Int PlayerPosition { get; set; }
        
        public int TurnCount { get; set; }
    }
}