using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;

namespace Cardevil.Card.InStage.Calculator
{
    public interface ICardStep { }

    public class ScoreStep : ICardStep
    {
        public IScoreOperator Operator { get; }
        public ScoreStep(IScoreOperator scoreOperator) => Operator = scoreOperator;
    }

    public class DiscardStep : ICardStep
    {
        public ICardState Card { get; }
        public DiscardStep(ICardState card) => Card = card;
    }

    public class MoveStep : ICardStep
    {
        public ICardState Card { get; }
        public PlayerMoveArgs Args { get; }

        public MoveStep(ICardState card)
        {
            Card = card;
            Args = PlayerMoveArgs.Get(card.Directions.Current!.Value);
        }
    }
}