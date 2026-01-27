using System.Collections.Generic;

namespace Cardevil.Cards.InStage
{
    public interface IReadOnlyEvaluationResultsModel
    {
        IReadOnlyList<EvaluationResult> History { get; }
        EvaluationResult? CurrentResult { get; }
    }
}