using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public abstract class ScoreEffectDefinition : EffectDefinition
    {
        [Header("점수 기본 설정")]
        [SerializeField] protected ScoreStepType scoreStepType;
        [SerializeField] protected ScoreOperatorType scoreOperatorType;
        [SerializeField] protected float value;

        protected string CommonDescription => scoreOperatorType switch
        {
            ScoreOperatorType.Plus => $"<color=#FFD700>+{value}점</color>을 부여합니다.",
            ScoreOperatorType.Multiply => $"<color=#FFD700>x{value}</color>를 부여합니다.",
            _ => "(정의되지 않음)"
        };
        
        public ScoreStepType ScoreStepType => scoreStepType;
        public ScoreOperatorType OperatorType => scoreOperatorType;
        public float Value => value;
    }
}