using Database.Generated;
using System;
using UnityEngine;

namespace Cardevil.Relics
{   
    public enum EffectType
    {
        OnAcquire, OnEnterStage, OnStage,
        OnDeath, OnStageEnd, OnEvaluation
    }

    public enum EffectExcuteType
    {
        None, immediate, Next, Permanent
    }

    public enum EffectDamageType
    {
        Plus, MultiplyAll, MultiplyRanking
    }

    [Serializable]
    public class RelicEffect
    {
        [SerializeField] EffectType _effectType;
        [SerializeField] RelicEffectOnEvaluationData _onEvaluationData;

        public EffectType EffectType => _effectType;
        public string EffectId => _effectType switch
        {
            EffectType.OnEvaluation => _onEvaluationData.EffectId,
            _ => ""
        };

        public RelicEffect(RelicEffectOnEvaluationData data)
        {
            _effectType = EffectType.OnEvaluation;
            _onEvaluationData = data;
        }
    }
}


