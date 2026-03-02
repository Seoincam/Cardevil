using Cardevil.Card.Common.Core;

namespace Cardevil.Card.InStage.Score.Step
{
    public interface IStepElement { }

    public class ScoreStepElement : IStepElement
    {
        public IScoreOperator Operator { get; }
        public ScoreStepElement(IScoreOperator scoreOperator) => Operator = scoreOperator;
    }

    public class DiscardStepElement : IStepElement
    {
        public ICardState Card { get; }
        public DiscardStepElement(ICardState card) => Card = card;
    }

    public class MoveStepElement : IStepElement
    {
        public ICardState Card { get; }
        public PlayerMoveArgs Args { get; }

        public MoveStepElement(ICardState card)
        {
            Card = card;
            Args = PlayerMoveArgs.Get(card.Directions.Current!.Value);
        }
    }
}