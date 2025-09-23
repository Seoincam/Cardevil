using UnityEngine;

namespace Cardevil.Relic
{
    [CreateAssetMenu(menuName = "Relics/RelicContainer")]
    public sealed class RelicContainerSO : ScriptableObject
    {
        [SerializeField] string _id;
        [SerializeField] TriggerTiming _trigger;
        [SerializeField] RelicSO[] _relics;
        
        [SerializeField] string _displayName;
        [SerializeField] string _displayDescription;

        [SerializeField] string comment;
    



        public string Id => _id;
        public TriggerTiming Trigger => _trigger;
        public RelicSO[] Relics => _relics;
        public string Name => _displayName;
        public string Description => _displayDescription;

        /*
        public void Apply()
        {
            bool canApply = _requireAll
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
            bool canApply = _requireAll
                ? _conditions.All(c => c.CanTrigger(resultCtx, evaluationCtx))
                : _conditions.Any(c => c.CanTrigger(resultCtx, evaluationCtx));

            if (canApply)
            {
                foreach (var effect in _effects)
                    effect.Apply(effectType, resultCtx, evaluationCtx);
            }
        }
        */



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