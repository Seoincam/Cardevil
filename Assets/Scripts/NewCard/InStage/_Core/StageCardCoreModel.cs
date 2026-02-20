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

        public StageCardCoreModel()
        {
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
                for (int n = 2; n <= 10; n++)
                {
                    if (color == CardColor.None) continue;
                    
                    var state = new CardSpec((uint)(10 + n), CardType.Attack)
                        .AddElements(
                            new BaseColorElement(color),
                            new BaseNumberElement(n)
                        ).State;
                    deck.Add(state);
                }
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