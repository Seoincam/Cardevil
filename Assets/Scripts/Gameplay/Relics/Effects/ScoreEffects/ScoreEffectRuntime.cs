using Cardevil.Card.InStage.Score;
using Cardevil.Card.InStage.Score.Step;
using Cardevil.Gameplay.Relics.Core;

namespace Cardevil.Gameplay.Relics.Effects.ScoreEffects
{
    public abstract class ScoreEffectRuntime : EffectRuntime, IScoreProvider
    {
        protected readonly ScoreEffectDefinition Definition;
        
        public int Id { get; set; } = -1;
        public ScoreStepType ScoreStepType => Definition.ScoreStepType;
        
        protected ScoreEffectRuntime(ScoreEffectDefinition definition, IRelicContext context) : base(context)
        {
            Definition = definition;
        }

        public abstract IScoreOperator GetScoreOperator(IScoreContext context);

        public override void OnActive()
        {
            Id = Context.ScoreProviderRegistry.Register(this);
        }

        public override void OnInactive()
        {
            Context.ScoreProviderRegistry.SafeUnregister(Id, this);
            Id = -1;
        }
    }
}