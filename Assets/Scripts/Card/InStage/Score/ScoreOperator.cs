using Cardevil.Card.InStage.Score.Step;
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
        IScoreSource Source { get; }
        
        /// <summary>
        /// 해당 연산자가 적용되기 전 점수.
        /// </summary>
        float PreviousScore { get; }
        
        /// <summary>
        /// 해당 연산자가 적용된 후 점수.
        /// </summary>
        float CurrentScore { get; }
        
        /// <summary>
        /// Sequencer에서 구성되는 시점에 호출됨.
        /// </summary>
        float Apply(float previousScore);
    }
    
    [Serializable]
    public class ScoreOperator : IScoreOperator
    {
        [field: SerializeField] public ScoreOperatorType Type { get; set; }
        [field: SerializeField] public float Value { get; set; }
        [field: SerializeField] public IScoreSource Source { get; set; }
        
        [field: SerializeField] public float PreviousScore { get; private set; }
        [field: SerializeField] public float CurrentScore { get; private set; }

        public float Apply(float previousScore)
        {
            PreviousScore = previousScore;

            float newScore = Type switch
            {
                ScoreOperatorType.Plus => previousScore + Value,
                ScoreOperatorType.Multiply => previousScore * Value,
                
                _ => throw new ArgumentOutOfRangeException()
            };

            CurrentScore = newScore;
            return newScore;
        }
        
        public static implicit operator ScoreStepElement(ScoreOperator scoreOperator) => new(scoreOperator);
    }
}