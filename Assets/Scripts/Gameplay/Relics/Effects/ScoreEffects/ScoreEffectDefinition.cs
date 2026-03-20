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
        [SerializeField] protected StepType stepType;
        [SerializeField] protected ScoreOperatorType operatorType;
        
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
                
                return operatorType switch
                {
                    ScoreOperatorType.Plus => $"<color=#FFD700>+{valueDesc}점</color>을 부여합니다.",
                    ScoreOperatorType.Multiply => $"<color=#FFD700>x{valueDesc}</color>를 부여합니다.",
                    _ => "(정의되지 않음)"
                };
            }
        }

        /// <summary>
        /// 에디터에서 사용하기 위한 실행 단계 열거형.
        /// 실제 사용시엔 <see cref="ScoreStepType"/>으로 변환함.
        /// </summary>
        public enum StepType
        {
            EachCard,
            PlusRelic,
            MultiplyRelic
        }

        public ScoreStepType ScoreStepType => stepType switch
        {
            StepType.EachCard => ScoreStepType.EachCard,
            StepType.PlusRelic => ScoreStepType.PlusRelic,
            StepType.MultiplyRelic => ScoreStepType.MultiplyRelic,
            
            _ => throw new ArgumentOutOfRangeException(nameof(stepType), stepType, null)
        };
        public ScoreOperatorType OperatorType => operatorType;

        public float GetCalculatedValue(RelicInstance context)
        {
            if (valueResolver == null) return baseValue;

            return baseValue * valueResolver.GetValue(context);
        }
    }
}