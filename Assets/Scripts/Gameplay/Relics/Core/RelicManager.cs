using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Bootstrap;
using Cardevil.Core.Systems.Save;
using Cardevil.Core.Utils;
using Cardevil.Test.DebugConsole;
using Cardevil.Test.DebugConsole.Commands;
using Cardevil.UI.GlobalNavigationBar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class RelicManager : ISaveLoad
    {
        private readonly Dictionary<string, RelicInstance> _ownedRelics = new();
        private readonly RelicDatabase _database;
        private readonly IRelicCommonContext _commonContext;

        public event Action<RelicInstance> RelicAdded;

        private RelicBar RelicBar => GlobalNavigationBar.Instance.RelicBar;

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

        public void AddRelic(string id)
        {
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
            
            var instance = new RelicInstance(definition, _commonContext);
            _ownedRelics.Add(id, instance);
            instance.Activate();
            
            var iconInstance = RelicBar.AddRelic(definition);
            instance.SetIcon(iconInstance);
            
            RelicAdded?.Invoke(instance);
            
            LogEx.Log($"유물 '{instance.Data.DisplayName}({instance.Data.Id})'를 획득했습니다!");
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
    }

    public interface IRelicCommonContext
    {
        PlayerStatus PlayerStatus { get; }
        ScoreProviderRegistry ScoreProviderRegistry { get; }
    }
    

}