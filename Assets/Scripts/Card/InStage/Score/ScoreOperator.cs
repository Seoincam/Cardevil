using System;
using UnityEngine;

namespace Cardevil.Card.InStage.Score
{
    public enum ScoreOperatorType : byte
    {
        Plus,
        Multiply,
    }

    public interface IScoreOperator
    {
        ScoreOperatorType Type { get; }
        float Value { get; }
        
        float Apply(float previousScore);
    }
    
    [Serializable]
    public class ScoreOperator : IScoreOperator
    {
        [field: SerializeField] public ScoreOperatorType Type { get; set; }
        [field: SerializeField] public float Value { get; set; }
        
        public float Apply(float previousScore)
        {
            switch (Type)
            {
                case ScoreOperatorType.Plus:
                    return previousScore + Value;
                    
                case ScoreOperatorType.Multiply:
                    return previousScore * Value;
                    
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}