namespace Cardevil.Cards.Evaluations
{
    /// <summary>
    /// 카드를 사용했을 때 반응해야 할 개체가 구현. 
    /// </summary>
    public interface IEvaluateVisual
    {
        /// <summary>
        /// Evaluation 시 반응.
        /// </summary>
        public void ExecuteEvaluationAction();
    }
}