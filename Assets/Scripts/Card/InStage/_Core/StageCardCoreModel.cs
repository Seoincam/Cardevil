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

        public StageCardCoreModel()
        {
            void DirectionTest()
            {
                var state1 = new CardSpec(1, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Up)
                    ).State;
                
                var state2 = new CardSpec(2, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Down)
                    ).State;
                
                var state3 = new CardSpec(3, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Left)
                    ).State;
                
                var state4 = new CardSpec(4, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Right)
                    ).State;
                
                var state5 = new CardSpec(5, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Left),
                        SelectableDirectionElement.Fixed(Direction.Right)
                    ).State;
                
                var state6 = new CardSpec(5, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Up),
                        SelectableDirectionElement.Fixed(Direction.Down),
                        SelectableDirectionElement.Fixed(Direction.Left),
                        SelectableDirectionElement.Fixed(Direction.Right)
                    ).State;
                
                deck.Add(state1);
                deck.Add(state2);
                deck.Add(state3);
                deck.Add(state4);
                deck.Add(state5);
                deck.Add(state6);
            }
            void ValueSelectionTest()
            { 
                var state1 = new CardSpec(1, CardType.Attack)
                    .AddElements(
                        new BaseColorElement(CardColor.Red),
                        new BaseNumberElement(2),
                        SelectableNumberElement.Fixed(3),
                        SelectableNumberElement.Fixed(4),
                        SelectableNumberElement.Fixed(5),
                        SelectableNumberElement.Fixed(6),
                        SelectableNumberElement.Fixed(7),
                        SelectableNumberElement.Fixed(8),
                        SelectableNumberElement.Fixed(9),
                        SelectableNumberElement.Fixed(10)
                    )
                    .State;
                var state2 = new CardSpec(2, CardType.Attack)
                    .AddElements(
                        new BaseColorElement(CardColor.Green),
                        SelectableColorElement.Random(),
                        new BaseNumberElement(3)
                    )
                    .State;
                var state3 = new CardSpec(3, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Up),
                        SelectableDirectionElement.Fixed(Direction.Down)
                    )
                    .State;
                var state4 = new CardSpec(6, CardType.Attack)
                    .AddElements(
                        new BaseColorElement(CardColor.Black),
                        new BaseNumberElement(7),
                        SelectableNumberElement.Fixed(8),
                        SelectableNumberElement.Fixed(9)
                    )
                    .State;
                var state5 = new CardSpec(5, CardType.Move)
                    .AddElements(
                        new BaseDirectionElement(Direction.Up)
                    )
                    .State;
                var state6 = new CardSpec(6, CardType.Attack)
                    .AddElements(
                        new BaseNumberElement(9),
                        new BaseColorElement(CardColor.Red),
                        SelectableColorElement.Fixed(CardColor.Green),
                        SelectableColorElement.Fixed(CardColor.Blue),
                        SelectableColorElement.Fixed(CardColor.Black)
                    )
                    .State;
                
                deck.Add(state1);
                deck.Add(state2);
                deck.Add(state3);
                deck.Add(state4);
                deck.Add(state5);
                deck.Add(state6);
            }
 
            DirectionTest();
            
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
                for (int n = 2; n <= 10; n++)
                {
                    if (color == CardColor.None) continue;
                    
                    var state = new CardSpec((10 + n), CardType.Attack)
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