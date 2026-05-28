using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems.Save;
using Cardevil.Core.Utils;
using Cardevil.Test.DebugConsole;
using Cardevil.Test.DebugConsole.Commands;
using Cardevil.UI.GlobalNavigationBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cardevil.Gameplay.Relics.Core
{
    public interface IRelicCommonContext
    {
        PlayerStatus PlayerStatus { get; }
        ScoreProviderRegistry ScoreProviderRegistry { get; }
    }
    
    [Serializable]
    public class RelicManager : ISaveLoad
    {
        private readonly Dictionary<string, RelicInstance> _ownedRelics = new();
        private readonly RelicDatabase _database;
        private readonly IRelicCommonContext _commonContext;

        public event Action<RelicInstance> RelicAdded;

        public RelicManager(PlayerStatus playerStatus, ScoreProviderRegistry scoreProviderRegistry)
        {
            const string databasePath = "ScriptableObjects/Relics/Relic Database";
            _database = AssetUtil.Load<RelicDatabase>(databasePath);
            if (!_database)
            {
                LogEx.LogError($"유물 데이터베이스를 찾을 수 없습니다. path: {databasePath}");
                return;
            }

            _database.RuntimeInitialize();
            _commonContext = new RelicCommonContext(playerStatus, scoreProviderRegistry);
        }

        
        /// <summary>
        /// 유물을 추가하고 활성화.
        /// </summary>
        public void AddRelic(string id)
        {
            if (!_database)
            {
                LogEx.LogWarning("유물 데이터베이스가 없어 획득할 수 없습니다.");
                return;
            }

            if (_ownedRelics.ContainsKey(id))
            {
                LogEx.LogWarning($"유물을 중복 획득했습니다. id: {id}");
                return;
            }

            var definition = _database.Get(id);
            if (definition == null)
            {
                LogEx.LogWarning($"DB에서 유물을 찾을 수 없습니다. id: {id}");
                return;
            }
            
            AddRelic(definition);
        }

        /// <summary>
        /// 유물을 추가하고 활성화.
        /// </summary>
        public void AddRelic(RelicDefinition definition)
        {
            if (definition == null)
            {
                LogEx.LogWarning("유물 정의가 없어 획득할 수 없습니다.");
                return;
            }

            if (_ownedRelics.ContainsKey(definition.Id))
            {
                LogEx.LogWarning($"유물을 중복 획득했습니다. id: {definition.Id}");
                return;
            }

            // Def에 해당하는 런타임 인스턴스 생성 및 활성화
            var instance = new RelicInstance(definition, _commonContext);
            _ownedRelics.Add(definition.Id, instance);
            instance.Activate();
            
            var relicBar = GlobalNavigationBar.Instance ? GlobalNavigationBar.Instance.RelicBar : null;
            var iconInstance = relicBar ? relicBar.AddRelic(definition) : null;
            instance.SetIcon(iconInstance);
            
            RelicAdded?.Invoke(instance);
            
            LogEx.Log($"유물 '{instance.Data.DisplayName}({instance.Data.Id})'를 획득했습니다!");
        }

        /// <summary>
        /// 희귀도에 해당하는 미획득 유물들을 반환.
        /// </summary>
        public List<RelicDefinition> GetRandomUnownedRelicsByRarity(RelicRarity targetRarity, int count)
        {
            if (!_database)
            {
                LogEx.LogWarning("유물 데이터베이스가 없어 유물을 뽑을 수 없습니다.");
                return new List<RelicDefinition>();
            }

            // 필터링
            var pool = _database.GetAll()
                .Where(def => def.Rarity == targetRarity && !_ownedRelics.ContainsKey(def.Id))
                .ToList();

            // 부족할 경우들 처리
            if (count > pool.Count)
                LogEx.LogWarning($"조건({targetRarity})에 부합하는 미획득 유물이 부족합니다. (요청: {count}, 가능: {pool.Count})");
            
            int actualCount = Mathf.Min(count, pool.Count);
            if (actualCount <= 0)
            {
                LogEx.LogWarning($"조건({targetRarity})에 부합하는 미획득 유물이 없습니다.");
                return new List<RelicDefinition>();
            }
            
            // 셔플 및 반환
            pool.ShuffleListInPlace();
            return pool.Take(actualCount).ToList();
        }

        /// <summary>
        /// 특정 인덱스의 유물을, 현재 리스트와 중복되지 않는 새로운 미획득 유물로 교체.
        /// </summary>
        public RelicDefinition RerollSingleRelic(
            RelicRarity targetRarity,
            List<RelicDefinition> currentOptions,
            int indexToReplace)
        {
            if (currentOptions == null ||
                indexToReplace < 0 ||
                indexToReplace >= currentOptions.Count)
            {
                LogEx.LogWarning($"리롤 인덱스가 올바르지 않습니다. index: {indexToReplace}");
                return null;
            }

            if (!_database)
            {
                LogEx.LogWarning("유물 데이터베이스가 없어 리롤할 수 없습니다.");
                return null;
            }

            // 필터링
            var excludedIds = currentOptions
                .Where(def => def != null)
                .Select(def => def.Id)
                .ToHashSet();

            var pool = _database.GetAll()
                .Where(def => def.Rarity == targetRarity
                              && !_ownedRelics.ContainsKey(def.Id)
                              && !excludedIds.Contains(def.Id))
                .ToList();

            // 부족할 경우 처리
            if (pool.Count <= 0)
            {
                LogEx.LogWarning($"리롤 불가. 조건({targetRarity})에 부합하는 미획득 유물이 없습니다.");
                return null;
            }
            
            // 랜덤 선택
            int randomIndex = RandomUtil.GetRandomInt(0, pool.Count);
            var newRelic = pool[randomIndex];
            
            // 원본 리스트에서 교체
            currentOptions[indexToReplace] = newRelic;
            return newRelic;
        }
        
        
        public void Save(GameSave currentSave)
        {
            currentSave.OwnedRelics.Clear();
            foreach (var relic in _ownedRelics.Values)
            {
                currentSave.OwnedRelics.Add(relic.CaptureSaveData());
            }
        }

        public void Load(GameSave currentSave)
        {
            foreach (var instance in _ownedRelics.Values)
            {
                instance.Deactivate();
            }
            _ownedRelics.Clear();

            foreach (var relicData in currentSave.OwnedRelics)
            {
                var definition = _database.Get(relicData.relicId);
                if (definition == null) continue;
                
                var instance = new RelicInstance(definition, _commonContext);
                instance.RestoreSaveData(relicData);
                
                _ownedRelics.Add(definition.Id, instance);
                instance.Activate();
            }
        }
        
        private class RelicCommonContext : IRelicCommonContext
        {
            public PlayerStatus PlayerStatus { get; }
            public ScoreProviderRegistry ScoreProviderRegistry { get; }
            
            public RelicCommonContext(
                PlayerStatus playerStatus,
                ScoreProviderRegistry scoreProviderRegistry)
            {
                PlayerStatus = playerStatus;
                ScoreProviderRegistry = scoreProviderRegistry;
            }
        }
        
        #region DebugConsole
        
        // [ConsoleCommand("addRelic", "유물을 획득합니다.", "addRelic <string: id>")]
        // private static void AddRelicCommand(string id)
        // {
        //     var relicManager = CardevilCore.Instance.GameManager.Relic;
        //     relicManager.AddRelic(id);
        // }

        [ConsoleCommand("printAllOwnedRelics", "획득한 모든 유물을 출력합니다.")]
        private static void PrintAllOwnedRelics()
        {
            var relicManager = CardevilCore.Instance.GameManager.Relic;
            var sb = new StringBuilder();
            
            foreach (var instance in relicManager._ownedRelics.Values)
            {
                sb.Append($"[{instance.Data.DisplayName}({instance.Data.Id})], "); 
            }
            sb.AppendLine($"총 유물 개수: {relicManager._ownedRelics.Count}");
            
            LogEx.Log($"현재 획득한 유물들: {sb}");
        }

        [ConsoleCommandClass]
        public class AddRelicCommand : IConsoleCommand
        {
            public string Command => "addRelic";
            public string Description => "유물을 획득합니다.";
            public string Signature => "addRelic <string: id>";

            public void Execute(params string[] args)
            {
                if (args.Length != 1)
                {
                    LogEx.LogWarning($"잘못된 명령어 사용법입니다. usage: {Signature}");
                    return;
                }
                string id = args[0];
                var relicManager = CardevilCore.Instance.GameManager.Relic;
                relicManager.AddRelic(id);
            }

            public void AutoComplete(Span<string> args, ref List<string> suggestions)
            {
                if (args.Length == 1)
                {
                    string partialId = args[0];
                    var relicManager = CardevilCore.Instance.GameManager.Relic;
                    foreach (var relicId in relicManager._database.Map.Keys)
                    {
                        if (relicId.StartsWith(partialId, StringComparison.OrdinalIgnoreCase))
                        {
                            suggestions.Add(relicId);
                        }
                    }
                }
            }
        }
        
        #endregion
    }
}
