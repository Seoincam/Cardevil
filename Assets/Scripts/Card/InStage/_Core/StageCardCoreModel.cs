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
        [SerializeReference] private List<INewCardState> deck = new();
        [SerializeReference] private List<INewCardState> discarded = new();
        
        public IReadOnlyList<INewCardState> Deck => deck;
        public IReadOnlyList<INewCardState> Discarded => discarded;

        public StageCardCoreModel(List<INewCardState> deepClonedStates)
        {
            deepClonedStates.ShuffleListInPlace();
            deck.AddRange(deepClonedStates);
        }

        public IReadOnlyList<INewCardState> Draw(int count = 1)
        {
            var states = new List<INewCardState>();
            
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

        public void Discard(INewCardState state)
        {
            discarded.Add(state);
        }

        public void Discard(IReadOnlyList<INewCardState> states)
        {
            discarded.AddRange(states);
        }

        public void Reroll(IReadOnlyList<INewCardState> states)
        {
            deck.AddRange(states);
        }

        public void ShuffleDeck()
        {
            deck.ShuffleListInPlace();
        }

        private bool TryDraw(out INewCardState state)
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