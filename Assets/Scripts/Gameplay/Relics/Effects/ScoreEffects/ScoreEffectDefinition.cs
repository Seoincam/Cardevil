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
        
        public ScoreStepType ScoreStepType => scoreStepType;
        public ScoreOperatorType OperatorType => scoreOperatorType;
        public float Value => value;
    }
}