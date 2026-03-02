using Cardevil.Card.Common.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.InStage.Score.Step
{
    public interface IScoreContext
    {
        IReadOnlyList<ICardState> Cards { get; }
        HandRankData HandRankData { get; }
        float CurrentScore { get; }
            
        ICardState CurrentCard { get; }
    }
    
    public class StepElementBuilder
    {
        private List<IScoreProvider> _providers;
        private Dictionary<ScoreStepType, List<IScoreProvider>> _providerMap;
        private ScoreContext _context;

        public void BuildContext(IReadOnlyList<ICardState> cards, HandRankData handRankData)
        {
            _context = new ScoreContext
            {
                Cards = cards, 
                HandRankData = handRankData
            };
        }

        public void ClearContext() => _context = null;

        public IReadOnlyList<IStepElement> BuildSteps(ScoreStepType type)
        {
            Debug.Assert(_context != null);
            
            if (type == ScoreStepType.EachCard)
            {
                return InternalBuildEachCardSteps();
            }

            var list = new List<ScoreStepElement>();
            
            foreach (var provider in GetSources(type))
            {
                var scoreOperator = provider.GetScoreOperator(_context);
                Apply(scoreOperator);
                
                var scoreStep = new ScoreStepElement(scoreOperator);
                list.Add(scoreStep);
            }

            return list;
        }
        
        private class ScoreContext : IScoreContext
        {
            public IReadOnlyList<ICardState> Cards { get; set; }
            public HandRankData HandRankData { get; set; }
            public float CurrentScore { get; set; }

            public ICardState CurrentCard { get; set; }
        }
        
        private IReadOnlyList<IStepElement> InternalBuildEachCardSteps()
        {
            Debug.Assert(_context != null);
            
            var list = new List<IStepElement>();

            foreach (var card in _context.Cards)
            {
                Debug.Assert(_context.Cards.Contains(card));
                
                _context.CurrentCard = card;
                
                if (card.IsMove)
                {
                    var moveStep = new MoveStepElement(card);
                    list.Add(moveStep);
                }
                else if (card.IsAttack)
                {
                    if (!_context.HandRankData.RankedCards.Contains(card))
                    {
                        var discardStep = new DiscardStepElement(card);
                        list.Add(discardStep);
                        continue;
                    }

                    var cardScoreOperator = new ScoreOperator
                    {
                        Type = ScoreOperatorType.Plus, 
                        Value = card.Numbers.Current!.Value, 
                        Source = card
                    };
                    Apply(cardScoreOperator);
                    list.Add((ScoreStepElement)cardScoreOperator);

                    foreach (var provider in GetSources(ScoreStepType.EachCard))
                    {
                        var scoreOperator = provider.GetScoreOperator(_context);
                        Apply(scoreOperator);
                        if (scoreOperator != null)
                        {
                            var scoreStep = new ScoreStepElement(scoreOperator);
                            list.Add(scoreStep);
                        }
                    }
                }
            }

            return list;
        }

        private IReadOnlyList<IScoreProvider> GetSources(ScoreStepType type)
        {
            if (!_providerMap.TryGetValue(type, out List<IScoreProvider> sources))
            {
                sources = _providers
                    .Where(s => s.ScoreStepType == type)
                    .ToList();
                
                _providerMap.Add(type, sources);
            }

            return sources;
        }

        private void Apply(IScoreOperator scoreOperator)
        {
            _context.CurrentScore = scoreOperator.Apply(_context.CurrentScore);
        }
    }
}