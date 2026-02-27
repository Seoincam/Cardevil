using Cardevil.NewCard.Common.Core;
using Cardevil.NewCard.InStage.Score;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.NewCard.InStage.Calculator
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
        public MoveStep(ICardState card) => Card = card;
    }

    
    public interface IEachCardScoreOperatorProvider
    {
        IScoreOperator OnCalculateCard(ICardState cardState);
    }

    public interface ITotalScoreOperatorProvider
    {
        IScoreOperator OnCalculateTotal();
    }

    
    public class CardUsingSequencer
    {
        public IReadOnlyList<IEachCardScoreOperatorProvider> EachCardScoreOperatorProviders;
        public IReadOnlyList<ITotalScoreOperatorProvider> TotalScoreOperatorProviders;

        public IReadOnlyList<ICardStep> Build(IReadOnlyList<ICardState> cards, HandRankData handRankData)
        {
            var list = new List<ICardStep>();
            
            foreach (var card in cards)
            {
                if (card.IsAttack)
                {
                    if (!handRankData.RankedCards.Contains(card))
                    {
                        list.Add(new DiscardStep(card));
                        continue;
                    }

                    var cardScoreStep = new ScoreStep(new ScoreOperator
                    {
                        Type = ScoreOperatorType.Plus, Value = (float)card.Numbers.Current!
                    });
                    list.Add(cardScoreStep);

                    foreach (var provider in EachCardScoreOperatorProviders)
                    {
                        var scoreOperator = provider.OnCalculateCard(card);
                        if (scoreOperator != null)
                        {
                            list.Add(new ScoreStep(scoreOperator));
                        }
                    }
                }
                else if (card.IsMove)
                {
                    list.Add(new MoveStep(card));
                }
            }

            foreach (var provider in TotalScoreOperatorProviders)
            {
                var scoreOperator = provider.OnCalculateTotal();
                if (scoreOperator != null)
                {
                    list.Add(new ScoreStep(scoreOperator));
                }
            }

            return list;
        }
    }
}