using Cardevil.Core.Systems.Save;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cardevil.Card.Common.Core
{
    [Serializable]
    public class CardRepository : ISaveLoad, INewGameInitializable
    {
        public const int CardCount = 50;
        
        [SerializeReference] private List<CardSpec> cards = new(CardCount);
        
        private readonly Dictionary<int, CardState> _stateCache = new(CardCount);
        
        public IReadOnlyList<CardSpec> Cards => cards;
        
        public void SetUpNewGame(GameSave save)
        {
            cards.Clear();
            _stateCache.Clear();

            var newSpecs = CreateStandardCardSpecs();
            foreach (var spec in newSpecs)
            {
                AddCardSpec(spec);
            }
        }
        
        public void Save(GameSave currentSave)
        {
            LogEx.LogWarning("카드 세이브로드 미구현 - @Seoincam");
        }

        public void Load(GameSave currentSave)
        {
            LogEx.LogWarning("카드 세이브로드 미구현 - @Seoincam");
        }

        /// <summary>
        /// 모든 카드의 최신 State 인터페이스 리스트를 반환.
        /// </summary>
        public List<ICardState> GetAllStates()
        {
            return _stateCache.Values
                .Cast<ICardState>()
                .ToList();
        }

        /// <summary>
        /// 모든 카드의 최신 State 인터페이스 리스트를 DeepClone해 반환.
        /// </summary>
        public List<ICardState> GetAllDeepClonedStates()
        {
            return _stateCache.Values
                .Select(state => state.DeepClone())
                .Cast<ICardState>()
                .ToList();
        }
        
        /// <summary>
        /// 특정 Id의 최신 State를 반환.
        /// </summary>
        public CardState GetState(int id)
        {
            if (_stateCache.TryGetValue(id, out var state))
            {
                return state;
            }
            
            var spec = cards.Find(c => c.ID == id);
            if (spec != null)
            {
                HandleSpecChanged(spec);
                return spec.State;
            }

            return null;
        }
        
        private void AddCardSpec(CardSpec spec)
        {
            cards.Add(spec);
            spec.SpecChanged += HandleSpecChanged;

            _stateCache[spec.ID] = spec.State;
        }
        
        // State 갱신
        private void HandleSpecChanged(CardSpec spec)
        {
            _stateCache[spec.ID] = spec.State;
            LogEx.Log($"Id {spec.ID}의 State가 자동 갱신됐음.");
        }

        // 기본 덱 생성
        private static List<CardSpec> CreateStandardCardSpecs()
        {
            var deckSpecs = new List<CardSpec>(CardCount);
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