namespace Cardevil.Cards.Evaluations
{
    public readonly struct EvaluationStep
    {
        public readonly EvaluationArg.EvaluationEffect Effect;
        public readonly float Value;
        public readonly float Before;
        public readonly float After;

        public EvaluationStep(EvaluationArg.EvaluationEffect effect, float value, float before, float after)
        {
            Effect = effect;
            Value = value;
            Before = before;
            After = after;
        }
    }
}