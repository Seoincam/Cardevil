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
        
        [Header("동적 수치 설정")]
        [SerializeField] protected float baseValue;
        [SerializeReference] protected IValueResolver valueResolver = new ConstantResolver();

        protected string CommonDescription
        {
            get
            {
                string valueDesc = valueResolver != null
                    ? valueResolver.GetDescription(baseValue)
                    : baseValue.ToString();
                
                return scoreOperatorType switch
                {
                    ScoreOperatorType.Plus => $"<color=#FFD700>+{valueDesc}점</color>을 부여합니다.",
                    ScoreOperatorType.Multiply => $"<color=#FFD700>x{valueDesc}</color>를 부여합니다.",
                    _ => "(정의되지 않음)"
                };
            }
        }
        
        public ScoreStepType ScoreStepType => scoreStepType;
        public ScoreOperatorType OperatorType => scoreOperatorType;

        public float GetCalculatedValue(RelicInstance context)
        {
            if (valueResolver == null) return baseValue;

            return baseValue * valueResolver.GetValue(context);
        }
    }
}