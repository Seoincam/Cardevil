using System.Collections.Generic;

namespace Cardevil.Cards.InStage.Model.ReadOnly
{
    public interface IReadOnlyEvaluationResultsModel
    {
        IReadOnlyList<EvaluationResult> History { get; }
        EvaluationResult? CurrentResult { get; }
    }
}