using Cardevil.Card.Common.Core.Upgrade;
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

        private readonly Dictionary<int, CardSpec> _specMap = new(CardCount);
        private readonly Dictionary<int, CardState> _stateCache = new(CardCount);
        private readonly Dictionary<int, NewCardState> _newStateCache = new(CardCount);

        private UpgradeNodeDatabaseSO _upgradeDatabase;
        
        public IReadOnlyList<CardSpec> Cards => cards;
        
        private CardRepository() { }
        public CardRepository(UpgradeNodeDatabaseSO upgradeDatabase)
        {
            _upgradeDatabase = upgradeDatabase;
        }
        
        public void SetUpNewGame(GameSave save)
        {
            cards.Clear();
            _stateCache.Clear();
            _newStateCache.Clear();

            var newSpecs = CreateStandardCardSpecs(_upgradeDatabase);
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

        public List<NewCardState> GetAllNewStates()
        {
            return _newStateCache.Values
                // .Cast<>()
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

        public List<INewCardState> GetAllDeepClonedNewStates()
        {
            return _newStateCache.Values
                .Select(state => state.DeepClone())
                .Cast<INewCardState>()
                .ToList();
        }

        /// <summary>
        /// 특정 Id의 Spec을 반환.
        /// </summary>
        public CardSpec GetSpec(int id)
        {
            if (_specMap.TryGetValue(id, out var spec))
            {
                return spec;
            }

            LogEx.LogError($"Spec을 찾을 수 없음! ID: {id}");
            return null;
        }

        /// <summary>
        /// 특정 Id의 Spec을 DeepClone해 반환.
        /// </summary>
        public CardSpec GetDeepClonedSpec(int id)
        {
            return GetSpec(id).DeepClone();
        }
        
        /// <summary>
        /// 특정 Id의 최신 State를 반환.
        /// </summary>
        // public CardState GetState(int id)
        // {
        //     if (_stateCache.TryGetValue(id, out var state))
        //     {
        //         return state;
        //     }
        //     
        //     var spec = cards.Find(c => c.ID == id);
        //     if (spec != null)
        //     {
        //         HandleSpecChanged(spec);
        //         return spec.State;
        //     }
        //
        //     return null;
        // }

        public NewCardState GetNewState(int id)
        {
            if (_newStateCache.TryGetValue(id, out var state))
            {
                return state;
            }
            
            var spec = cards.Find(c => c.ID == id);
            if (spec != null)
            {
                HandleSpecChanged(spec);
                return spec.NewState;
            }
            
            return null;
        }

        /// <summary>
        /// 특정 Id의 최신 State를 DeepClone해 반환.
        /// </summary>
        // public CardState GetDeepClonedState(int id)
        // {
        //     return GetState(id)?.DeepClone();
        // }

        public NewCardState GetDeepClonedNewState(int id)
        {
            return GetNewState(id)?.DeepClone();
        }
        
        private void AddCardSpec(CardSpec spec)
        {
            cards.Add(spec);
            spec.SpecChanged += HandleSpecChanged;

            _specMap[spec.ID] = spec;
            _newStateCache[spec.ID] = spec.NewState;
        }
        
        // State 갱신
        private void HandleSpecChanged(CardSpec spec)
        {
            // _stateCache[spec.ID] = spec.State;
            _newStateCache[spec.ID] = spec.NewState;
            LogEx.Log($"Id {spec.ID}의 State가 자동 갱신됐음.");
        }

        // 기본 덱 생성
        private static List<CardSpec> CreateStandardCardSpecs(UpgradeNodeDatabaseSO upgradeDatabase)
        {
            var deckSpecs = new List<CardSpec>(CardCount);
            int nextID = 0;

            var noneUpgradeNode = upgradeDatabase.GetNode(UpgradePath.None, 0);
            var multiNumberFinalUpgradeNode = upgradeDatabase.GetNode(UpgradePath.MultiNumber, 3);
            var multiDirectionFinalUpgradeNode = upgradeDatabase.GetNode(UpgradePath.MultiDirection, 2);
            
            // 공격 카드
            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                if (color == CardColor.None) continue;

                // 기본 공격 카드 스펙
                for (int n = 2; n <= 10; n++)
                {
                    var defaultAttackSpec = new CardSpec(nextID++, CardType.Attack, noneUpgradeNode)
                        .AddElements(
                            new BaseColorElement(color),
                            new BaseNumberElement(n)
                        );
                    deckSpecs.Add(defaultAttackSpec);
                }

                // 오망성 카드 스펙
                var starSpec = new CardSpec(nextID++, CardType.Attack)
                    .AddElements(new BaseColorElement(color))
                    .ApplyUpgradeNode(multiNumberFinalUpgradeNode);
                deckSpecs.Add(starSpec);
            }

            // 이동 카드
            
            // 기본 이동 카드 스펙
            foreach (Direction dir in new [] { Direction.Up , Direction.Down, Direction.Left, Direction.Right })
            {
                for (int i = 0; i < 2; i++)
                {
                    var defaultMoveSpec = new CardSpec(nextID++, CardType.Move, noneUpgradeNode)
                        .AddElements(new BaseDirectionElement(dir));
                    deckSpecs.Add(defaultMoveSpec);
                }
            }
            
            // 네 방향 이동 카드 스펙
            for (int i = 0; i < 2; i++)
            {
                var fourDirectionSpec = new CardSpec(nextID++, CardType.Move, multiDirectionFinalUpgradeNode);
                deckSpecs.Add(fourDirectionSpec);
            }

            return deckSpecs;
        }
    }
}