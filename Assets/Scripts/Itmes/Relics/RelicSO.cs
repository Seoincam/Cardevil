using Cardevil.Cards;
using System.Linq;
using UnityEngine;

namespace Cardevil.Relic
{
    [CreateAssetMenu(menuName = "Relics/Relic")]
    public class RelicSO : ScriptableObject
    {
        [SerializeField] protected string _id;
        [SerializeField] protected TriggerTiming _trigger;

        [SerializeField, Tooltip("조건을 모두 만족시켜야하는가?")]
        protected bool _requireAllConditions;
        [SerializeField] protected RelicConditionSO[] _conditions; 
        [SerializeField] protected RelicEffectSO[] _effects;

        [SerializeField] string comment;



        public string Id => _id;
        public TriggerTiming Trigger => _trigger;
        public RelicConditionSO[] Conditions;
        public RelicEffectSO[] Effects => _effects;


        public void Apply()
        {
            bool canApply = _requireAllConditions
                ? _conditions.All(c => c.CanTrigger())
                : _conditions.Any(c => c.CanTrigger());

            if (canApply)
            {
                foreach (var effect in _effects)
                    effect.Apply();
            }
        }

        public void Apply(CardResultEvaluator.EffectType effectType, CardResultContext resultCtx, CardEvaluationContext evaluationCtx)
        {
            bool canApply = _requireAllConditions
                ? _conditions.All(c => c.CanTrigger(resultCtx, evaluationCtx))
                : _conditions.Any(c => c.CanTrigger(resultCtx, evaluationCtx));

            if (canApply)
            {
                foreach (var effect in _effects)
                    effect.Apply(effectType, resultCtx, evaluationCtx);
            }
        }



        public enum TriggerTiming
        {
            OnAcquire, // 즉시 (획득 시 1회 발동)
            OnStageStart, // 스테이지 시작시
            OnStage, // 스테이지 내
            OnDeath, // 죽었을시
            OnStageEnd, // 스테이지 종료시
            OnResultEvaluate, // 카드 사용 결과 계산시
        }
    }
}