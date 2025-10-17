using Database.Generated;
using System;
using UnityEngine;
using Cardevil.Attributes;
using Cardevil.Cards.Data;
using System.Collections.Generic;

namespace Cardevil.Relics
{   
    public enum EffectType
    {
        OnAcquire, OnEnterStage, OnStage,
        OnDeath, OnStageEnd, OnEvaluation
    }

    public enum EffectExcute
    {
        None, immediate, Next, Permanent
    }

    public enum EffectEvaluation
    {
        None, Plus, CardDamagePlus, MultiplyAll, MultiplyRanking
    }

    [Serializable]
    public sealed class EvaluationRelicEffect : RelicEffectBase
    {
        [Header("OnEvaluation")]
        [SerializeField, VisibleOnly] EffectType _effectType;
        [SerializeField, VisibleOnly] OnEvaluationDataValues _onEvaluationValues;
        

        public EffectType EffectType => _effectType;
        public string EffectId => effectId;

        public OnEvaluationDataValues OnEvaluationValues => _onEvaluationValues;

        public EvaluationRelicEffect(RelicEffectOnEvaluationData data)
        {
            _effectType = EffectType.OnEvaluation;
            effectId = data.EffectId;

            _onEvaluationValues = new(
                true, data.Possibility, data.TriggerHp,
                data.ExecuteType, data.TriggerRanking, data.EvaluationType,
                data.EffectValue, data.ExecutionCount, data.TargetRankings
                );
        }

        public void Init(Relic relic)
        {
            this.relic = relic;
        }

        [Serializable]
        public readonly struct OnEvaluationDataValues
        {
            public readonly bool IsSet;
            public readonly float Possibility;
            public readonly int TriggerHp;
            public readonly EffectExcute ExecuteType;
            public readonly HandRanking TriggerRanking;
            public readonly EffectEvaluation EvaluationType;
            public readonly float EffectValue;
            public readonly int ExecutionCount;
            public readonly List<HandRanking> TargetRankings;

            public OnEvaluationDataValues(
                bool isSet, float possibility, int triggerHp,
                EffectExcute executeType, HandRanking triggerRanking,
                EffectEvaluation evaluationType, float effectValue,
                int executionCount, List<HandRanking> targetRankings)
            {
                IsSet = isSet;
                Possibility = possibility;
                TriggerHp = triggerHp;
                ExecuteType = executeType;
                TriggerRanking = triggerRanking;
                EvaluationType = evaluationType;
                EffectValue = effectValue;
                ExecutionCount = executionCount;
                TargetRankings = targetRankings;
            }
        }

        /// <summary>
        /// data의 조건들을 바탕으로 Evaluation시 체크.
        /// </summary>
        public bool CanTriggerOnEvaluation(HandRanking ranking)
        {
            /*
            if (_effectType != EffectType.OnEvaluation)
                return false;

            var data = _onEvaluationValues;

            // 확률
            if (data.Possibility < UnityEngine.Random.value)
                return false;

            // HP (0이면 무시)
            if (data.TriggerHp != 0 &&
                Managers.Game?.PlayerStatus?.CurrentHp != data.TriggerHp)
                return false;

            // var resultCtx = Managers.Card.ResultCtx;
            IReadOnlyStageEvaluationResultsModel resultCtx;
            switch (data.ExecuteType)
            {
                case EffectExcute.immediate:
                    // TriggerRanking == None이면 모든 랭킹 허용
                    if (data.TriggerRanking != HandRanking.None &&
                        data.TriggerRanking != ranking)
                        return false;
                    return true;

                case EffectExcute.Next:
                    // 현재 랭킹이 타겟에 포함되어야 함
                    if (data.TargetRankings.Count == 0 || !data.TargetRankings.Contains(ranking))
                        return false;

                    int remaining = data.ExecutionCount;

                    // 최근부터 과거로 스캔하는 방식으로 체크
                    for (int i = resultCtx.History.Count - 1; i >= 0; i--)
                    {
                        var result = resultCtx.History[i];
                        if (result is not { } r) continue;

                        if (r.Ranking == data.TriggerRanking)
                            return remaining > 0;

                        if (data.TargetRankings.Contains(r.Ranking))
                        {
                            remaining--;
                            if (remaining <= 0) return false;
                        }
                    }
                    return false;

                case EffectExcute.Permanent:
                    // 현재 랭킹이 타겟에 포함
                    if (data.TargetRankings.Count == 0 || !data.TargetRankings.Contains(ranking))
                        return false;

                    return resultCtx.History.Any(r => r?.Ranking == data.TriggerRanking);

                case EffectExcute.None:
                default:
                    return false;
                    */
            return false;
        }
    }
}



