using Cardevil.Attributes;
using Cardevil.NewCard.Core;
using Cardevil.Utils.Directions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Cardevil.NewCard.Config
{
    [CreateAssetMenu(menuName = "Cards/Config/Deck", fileName = "DeckConfig")]
    public class DeckConfig : ScriptableObject
    {
        [Header("Debug")] 
        [SerializeField] private int debug_deckCount;
        
        [Header("Attack Cards")]
        [SerializeField] private CardColor[] colors = { CardColor.Red, CardColor.Green, CardColor.Blue, CardColor.Black };

        [Space]
        [SerializeField, Min(0)] private int numberMin = 2;
        [SerializeField, Min(0)] private int numberMax = 10;

        [Space]
        [SerializeField, Min(0)] private int starCardForEachColor = 1;

        [Header("Move Cards")] 
        [SerializeField, Min(0)] private int moveCardsPerDirection = 2;
        [SerializeField, Min(0)] private int fourDirectionMoveCards = 2;

        public int Debug_DeckCount
        {
            get
            {
                SyncCount();
                return debug_deckCount;
            }
        }

        
#if UNITY_EDITOR
        private void OnValidate()
        {
            SyncCount();
        }
        
        private void SyncCount()
        {
            debug_deckCount = (numberMax - numberMin + 1 + starCardForEachColor) * colors.Length +
                              4 * moveCardsPerDirection + fourDirectionMoveCards;
        }
#else
        private void SyncCount() { }
#endif

        public List<CardSpec> CreateDefaultSpecs()
        {
            var specs = new List<CardSpec>();
            uint id = 1;
            
            // Attack
            foreach (var color in colors)
            {
                for (int n = numberMin; n <= numberMax; n++)
                {
                    var spec = new CardSpec(id++, CardType.Attack)
                        .AddElements(
                            new BaseColorElement(color),
                            new BaseNumberElement(n)
                        );
                    specs.Add(spec);
                }

                for (int i = 0; i < starCardForEachColor; i++)
                {
                    using var _ = ListPool<ISpecElement>.Get(out var elements);
                    elements.Add(new BaseColorElement(color));
                    
                    for (int n = numberMin; n <= numberMax; n++)
                    {
                        elements.Add(SelectableNumberElement.Fixed(n));
                    }

                    var spec = new CardSpec(id++, CardType.Attack, elements);
                    specs.Add(spec);
                }
            }
            
            // Move
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction == Direction.None) continue;

                for (int i = 0; i < moveCardsPerDirection; i++)
                {
                    var spec = new CardSpec(id++, CardType.Move)
                        .AddElements(new BaseDirectionElement(direction));
                    specs.Add(spec);
                }
            }

            for (int i = 0; i < fourDirectionMoveCards; i++)
            {
                using var _ = ListPool<ISpecElement>.Get(out var elements);
                elements.Add(SelectableDirectionElement.Fixed(Direction.Up));
                elements.Add(SelectableDirectionElement.Fixed(Direction.Down));
                elements.Add(SelectableDirectionElement.Fixed(Direction.Left));
                elements.Add(SelectableDirectionElement.Fixed(Direction.Right));
                
                var spec = new CardSpec(id++, CardType.Move, elements);
                specs.Add(spec);
            }

            return specs;
        }
    }
}