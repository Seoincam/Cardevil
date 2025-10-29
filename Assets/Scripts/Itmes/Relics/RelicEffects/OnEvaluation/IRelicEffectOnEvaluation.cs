using Cardevil.Cards.InStage.Model.ReadOnly;

namespace Cardevil.Relics.OnEvaluation
{
    public interface IRelicEffectOnEvaluation
    {
        bool CanTrigger(IReadOnlyEvaluationResultsModel resultModel);
    }
}