using Cardevil.NewCard.Common.Core;
using Cardevil.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.NewCard.InStage
{
    [Serializable]
    public class StageCardCoreModel
    {
        [SerializeReference] private List<ICardState> deck = new();
        [SerializeReference] private List<ICardState> discarded = new();
        
        public IReadOnlyList<ICardState> Deck => deck;
        public IReadOnlyList<ICardState> Discarded => discarded;

        public ICardState PopCardData()
        {
            if (deck.Count == 0)
            {
                LogEx.Log("뽑을 카드가 존재하지 않음.");
                return null;
            }
            
            var state = deck[0];
            deck.RemoveAt(0);
            
            return state;
        }

        public void Discard(ICardState state)
        {
            discarded.Add(state);
        }

        public void Discard(IReadOnlyList<ICardState> states)
        {
            discarded.AddRange(states);
        }
    }
}