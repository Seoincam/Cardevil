using Cardevil.Core.Systems.Save;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public class CardRepository : ISaveLoad, INewGameInitializable
    {
        [SerializeReference] private List<CardSpec> cards = new();
        
        public IReadOnlyList<CardSpec> Cards => cards;
        
        public void SetUpNewGame(GameSave save)
        {
            cards.Clear();
            cards.AddRange(CreateStandardCardSpecs());
        }
        
        public void Save(GameSave currentSave)
        {
            LogEx.LogWarning("카드 세이브로드 미구현 - @Seoincam");
        }

        public void Load(GameSave currentSave)
        {
            LogEx.LogWarning("카드 세이브로드 미구현 - @Seoincam");
        }

        private static List<CardSpec> CreateStandardCardSpecs()
        {
            var deckSpecs = new List<CardSpec>(50);
            int nextID = 0;
            
            // 공격 카드
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color == CardColor.None) continue;

                // 기본 공격 카드 스펙
                for (int n = 2; n <= 10; n++)
                {
                    var defaultAttackSpec = new CardSpec(nextID++, CardType.Attack)
                        .AddElements(
                            new BaseColorElement(color),
                            new BaseNumberElement(n)
                        );
                    deckSpecs.Add(defaultAttackSpec);
                }

                // 오망성 카드 스펙
                var starSpec = new CardSpec(nextID++, CardType.Attack)
                    .AddElements(new BaseColorElement(color));

                for (int n = 2; n <= 10; n++)
                {
                    starSpec.AddElements(SelectableNumberElement.Fixed(n));
                }
                deckSpecs.Add(starSpec);
            }

            // 이동 카드
            
            // 기본 이동 카드 스펙
            foreach (Direction dir in new [] { Direction.Up , Direction.Down, Direction.Left, Direction.Right })
            {
                for (int i = 0; i < 2; i++)
                {
                    var defaultMoveSpec = new CardSpec(nextID++, CardType.Move)
                        .AddElements(new BaseDirectionElement(dir));
                    deckSpecs.Add(defaultMoveSpec);
                }
            }
            
            // 네 방향 이동 카드 스펙
            for (int i = 0; i < 2; i++)
            {
                var fourDirectionSpec = new CardSpec(nextID++, CardType.Move)
                    .AddElements(
                        SelectableDirectionElement.Fixed(Direction.Up),
                        SelectableDirectionElement.Fixed(Direction.Down),
                        SelectableDirectionElement.Fixed(Direction.Left),
                        SelectableDirectionElement.Fixed(Direction.Right)
                    );
                deckSpecs.Add(fourDirectionSpec);
            }

            return deckSpecs;
        }
    }
}