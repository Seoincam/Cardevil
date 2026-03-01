using Cardevil.Card.Common.Core;
using Cardevil.Card.InStage.Score;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cardevil.Card.InStage.Calculator
{
    public interface IEachCardScoreOperatorProvider
    {
        IScoreOperator OnCalculateCard(ICardState cardState);
    }

    public interface ITotalScoreOperatorProvider
    {
        IScoreOperator OnCalculateTotal();
    }

    
    public class StepSequencer
    {
        public IReadOnlyList<IEachCardScoreOperatorProvider> EachCardScoreOperatorProviders;
        public IReadOnlyList<ITotalScoreOperatorProvider> TotalScoreOperatorProviders;

        public IReadOnlyList<ICardStep> BuildPlayerSteps(IReadOnlyList<ICardState> cards, HandRankData handRankData)
        {
            var list = new List<ICardStep>();
            
            foreach (var card in cards)
            {
                if (card.IsMove)
                {
                    list.Add(new MoveStep(card));
                }
                else if (card.IsAttack)
                {
                    if (!handRankData.RankedCards.Contains(card))
                    {
                        list.Add(new DiscardStep(card));
                        continue;
                    }

                    var cardScoreStep = new ScoreStep(new ScoreOperator
                    {
                        Type = ScoreOperatorType.Plus, 
                        Value = card.Numbers.Current!.Value
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
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(card), "Card Type is Invalid.");
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

        public IReadOnlyList<ICardStep> BuildEnemySteps()
        {
            var list = new List<ICardStep>();
            
            

            return list;
        }
    }
}