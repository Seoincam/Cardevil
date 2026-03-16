using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;
using System;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Effects.ScoreProvider
{
    [Serializable]
    public abstract class ScoreEffectBase : EffectBase, IScoreProvider
    {
        [Header("Common")]
        [SerializeField] protected ScoreStepType scoreStepType;
        [SerializeField] protected ScoreOperatorType scoreOperatorType;
        [SerializeField] protected float value;

        protected ScoreEffectBase() { }
        protected ScoreEffectBase(
            IRelicContext context, 
            ScoreStepType scoreStepType, 
            ScoreOperatorType scoreOperatorType, 
            float value, 
            int id) 
            : base(context)
        {
            this.scoreStepType = scoreStepType;
            this.scoreOperatorType = scoreOperatorType;
            this.value = value;
            Id = id;
        }

        public int Id { get; set; }
        public ScoreStepType ScoreStepType => scoreStepType;
        
        public IScoreOperator GetScoreOperator(IScoreContext context)
        {
            return InternalGetScoreOperator(context);
        }

        public override void OnInactive()
        {
            Context.ScoreProviderRegistry.SafeUnregister(Id, this);
        }

        public override void OnActive()
        {
            Id = Context.ScoreProviderRegistry.Register(this);
        }

        protected abstract IScoreOperator InternalGetScoreOperator(IScoreContext context);
    }
}