using Cardevil.Cards.Evaluations;
using Database.Generated;
using System;
using System.Linq;
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
        None, Plus, MultiplyAll, MultiplyRanking
    }

    [Serializable]
    public class RelicEffect
    {
        [SerializeField] Relic relic;
        [SerializeField] EffectType _effectType;
        [SerializeField] RelicEffectOnEvaluationData _onEvaluationData;

        public EffectType EffectType => _effectType;
        public string EffectId => _effectType switch
        {
            EffectType.OnEvaluation => _onEvaluationData.EffectId,
            _ => ""
        };
        public RelicEffectOnEvaluationData OnEvaluationData => _onEvaluationData;

        public RelicEffect(RelicEffectOnEvaluationData data)
        {
            _effectType = EffectType.OnEvaluation;
            _onEvaluationData = data;
        }

        public void Init(Relic relic)
        {
            this.relic = relic;
        }

        /// <summary>
        /// data의 조건들을 바탕으로 Evaluation시 체크.
        /// </summary>
        public bool CanTriggerOnEvaluation(HandRanking ranking)
        {
            if (_effectType != EffectType.OnEvaluation
                || _onEvaluationData == null)
                return false;

            var data = _onEvaluationData;

            // 확률
            if (data.Possibility < UnityEngine.Random.value)
                return false;

            // HP (0이면 무시)
            if (data.TriggerHp != 0 &&
                Managers.Game?.PlayerStatus?.CurrentHp != data.TriggerHp)
                return false;

            var resultCtx = Managers.Card.ResultCtx;
            switch (data.ExecuteType)
            {
                case EffectExcuteType.immediate:
                    // TriggerRanking == None이면 모든 랭킹 허용
                    if (data.TriggerRanking != HandRanking.None &&
                        data.TriggerRanking != ranking)
                        return false;
                    return true;

                case EffectExcuteType.Next:
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

                case EffectExcuteType.Permanent:
                    // 현재 랭킹이 타겟에 포함
                    if (data.TargetRankings.Count == 0 || !data.TargetRankings.Contains(ranking))
                        return false;

                    return resultCtx.History.Any(r => r?.Ranking == data.TriggerRanking);

                case EffectExcuteType.None:
                default:
                    return false;
            }
        }
    }
}


