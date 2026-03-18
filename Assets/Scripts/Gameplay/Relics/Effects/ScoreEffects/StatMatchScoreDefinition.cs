using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    [Serializable]
    public class StatMatchScoreDefinition : ScoreEffectDefinition
    {
        [Header("스탯 일치 보너스 설정")]
        [SerializeField] private PlayerStatType targetStat;
        [SerializeField] private int targetValue;

        public override string EditorName => "점수/스탯 일치 보너스";
        public override string EditorDescription => $"<color=#FFD700>{targetStat} 스탯</color>이 <color=#FFD700>{targetValue}</color>일 경우, <color=#FFD700>{Value}점({OperatorType})</color>을 부여합니다.";

        
        public override EffectRuntime CreateRuntimeInstance(IRelicContext context) => new Runtime(this, context);

        [Serializable]
        public class Runtime : ScoreEffectRuntime
        {
            private StatMatchScoreDefinition _definition;
            
            public Runtime(StatMatchScoreDefinition definition, IRelicContext context) : base(definition, context)
            {
                _definition = definition;
            }

            public override IScoreOperator GetScoreOperator(IScoreContext context)
            {
                if (Context.PlayerStatus.GetFinalValue(_definition.targetStat) != _definition.targetValue)
                {
                    return null;
                }
                
                return new ScoreOperator { Type = Definition.OperatorType, Value = Definition.Value, Source = this };
            }
        }
    }
}