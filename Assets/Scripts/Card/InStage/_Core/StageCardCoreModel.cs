using Cardevil.Card.Common.Core;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.InStage
{
    [Serializable]
    public class StageCardCoreModel
    {
        [SerializeReference] private List<ICardState> deck = new();
        [SerializeReference] private List<ICardState> discarded = new();
        
        public IReadOnlyList<ICardState> Deck => deck;
        public IReadOnlyList<ICardState> Discarded => discarded;

        public StageCardCoreModel(List<ICardState> deepClonedStates)
        {
            deepClonedStates.ShuffleListInPlace();
            deck.AddRange(deepClonedStates);
        }

        public IReadOnlyList<ICardState> Draw(int count = 1)
        {
            var states = new List<ICardState>();
            
            for (int i = 0; i < count; i++)
            {
                if (!TryDraw(out var state))
                {
                    return states;
                }
                
                state.ResolveValues();
                states.Add(state);
            }
            
            return states;
        }

        public void Discard(ICardState state)
        {
            discarded.Add(state);
        }

        public void Discard(IReadOnlyList<ICardState> states)
        {
            discarded.AddRange(states);
        }

        public void Reroll(IReadOnlyList<ICardState> states)
        {
            deck.AddRange(states);
        }

        public void ShuffleDeck()
        {
            deck.ShuffleListInPlace();
        }

        private bool TryDraw(out ICardState state)
        {
            if (deck.Count == 0)
            {
                LogEx.Log("뽑을 카드가 없음.");
                state = null;
                return false;
            }
            
            state = deck[0];
            deck.RemoveAt(0);
            return true;
        }
    }
}