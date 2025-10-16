using Cardevil.Cards.InStage.Model;
using System.Collections.Generic;

namespace Cardevil.Cards.InStage.Model.ReadOnly
{
    public interface IReadOnlyStageEvaluationResultsModel
    {
        IReadOnlyList<EvaluationResult> History { get; }
        EvaluationResult CurrentResult { get; }
    }
}