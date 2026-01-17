using Cardevil.Cards.Core;
using Cardevil.Cards.Evaluation;
using Cardevil.Cards.InStage;

namespace Cardevil.Relics.OnEvaluation
{
    public interface IRelicEffectOnEvaluation
    {
        bool IsPlus { get; }
        
        /// <summary>
        /// 효과 발동 가능 여부 평가.
        /// 현재 족보와 평가 결과 모델을 기반으로 조건 검증.
        /// </summary>
        /// <returns>효과 발동 가능 여부</returns>
        bool CanTrigger(HandRanking currentHandRanking, IReadOnlyEvaluationResultsModel resultModel);
        
        /// <summary>
        /// 평가 스텝 생성.
        /// <see cref="EvaluationStep"/> 구성 및 반환.
        /// </summary>
        /// <returns>생성된 평가 스텝</returns>
        EvaluationStep MakeEvaluationStep();
    }
}