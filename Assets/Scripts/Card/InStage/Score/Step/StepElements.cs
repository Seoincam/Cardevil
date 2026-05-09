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
        public INewCardState Card { get; }
        public DiscardStepElement(INewCardState card) => Card = card;
    }

    public class MoveStepElement : IStepElement
    {
        public INewCardState Card { get; }
        public PlayerMoveArgs Args { get; }

        public MoveStepElement(INewCardState card)
        {
            Card = card;
            Args = PlayerMoveArgs.Get(card.DirectionList.FixedValue);
        }
    }
}