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
        HandRank HandRank { get; }
    }
    
    public interface IScoreProviderRegistry
    {
        /// <summary>
        /// Provider의 ScoreStepType을 확인해 적절한 리스트를 추가.
        /// </summary>
        /// <returns> 등록된 Id. 등록 해제에 사용됨. </returns>
        int Register(IScoreProvider provider);
        
        /// <summary>
        /// 리스트에서 제거.
        /// </summary>
        void SafeUnregister(int id, IScoreProvider provider);
        
        /// <summary>
        /// 해당 타입의 모든 Provider 리스트를 반환.
        /// </summary>
        IReadOnlyList<IScoreProvider> GetProviders(ScoreStepType type);
    }
    
    public class StepElementBuilder
    {
        private readonly IScoreProviderRegistry _providerRegistry;
        
        private ScoreContext _context;

        public StepElementBuilder(IScoreProviderRegistry registry)
        {
            _providerRegistry = registry;
        }

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
            Debug.Assert(_context != null, "_context == null");
            
            return type switch
            {
                ScoreStepType.EachCard => InternalBuildEachCardSteps(),
                _ => InternalBuildSteps(type)
            };
        }
        
        private class ScoreContext : IScoreContext
        {
            public IReadOnlyList<ICardState> Cards { get; set; }
            public HandRankData HandRankData { get; set; }
            public float CurrentScore { get; set; }

            public ICardState CurrentCard { get; set; }
            public HandRank HandRank => HandRankData.HandRank;
        }

        private IReadOnlyList<IStepElement> InternalBuildSteps(ScoreStepType type)
        {
            var providers = _providerRegistry.GetProviders(type);
            if (providers == null) return null;
            
            var list = new List<IStepElement>();
            
            foreach (var provider in _providerRegistry.GetProviders(type))
            {
                var scoreOperator = provider.GetScoreOperator(_context);
                Apply(scoreOperator);
                
                var scoreStep = new ScoreStepElement(scoreOperator);
                list.Add(scoreStep);
            }

            return list;
        }
        
        private IReadOnlyList<IStepElement> InternalBuildEachCardSteps()
        {
            Debug.Assert(_context != null);
            
            var list = new List<IStepElement>();

            foreach (var card in _context.Cards)
            {
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
                    
                    var providers = _providerRegistry.GetProviders(ScoreStepType.EachCard);
                    if (providers == null) continue;
                    
                    foreach (var provider in providers)
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

        private void Apply(IScoreOperator scoreOperator)
        {
            if (scoreOperator == null) return;
            
            _context.CurrentScore = scoreOperator.Apply(_context.CurrentScore);
        }
    }
}