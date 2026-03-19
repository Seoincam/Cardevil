using Cardevil.Card.InStage.Score.Step;
using Cardevil.Core.Utils;
using System;
using System.Collections.Generic;

namespace Cardevil.Gameplay.Relics.Core
{
    [Serializable]
    public class RelicManager
    {
        private readonly Dictionary<string, RelicInstance> _ownedRelics = new();
        private readonly RelicDatabase _database;
        private readonly IRelicContext _context;
        
        private PlayerStatus _playerStatus;
        private ScoreProviderRegistry _scoreProviderRegistry;

        public event Action<RelicInstance> RelicAdded;

        public RelicManager(PlayerStatus playerStatus, ScoreProviderRegistry scoreProviderRegistry)
        {
            const string databasePath = "Assets/Resources/ScriptableObjects/Relics/Relic Database.asset";
            _database = AssetUtil.Load<RelicDatabase>(databasePath);
            if (!_database)
            {
                LogEx.LogError($"유물 데이터베이스를 찾을 수 없습니다. path: {databasePath}");
                return;
            }
            
            _playerStatus = playerStatus;
            _scoreProviderRegistry = scoreProviderRegistry;
            
            _database.RuntimeInitialize();
            _context = new RelicContext(_playerStatus, scoreProviderRegistry);
        }

        public void AddRelic(string id)
        {
            if (_ownedRelics.ContainsKey(id))
            {
                LogEx.LogWarning($"유물을 중복 획득했습니다. id: {id}");
                return;
            }

            var so = _database.Get(id);
            if (!so)
            {
                LogEx.LogWarning($"DB에서 유물을 찾을 수 없습니다. id: {id}");
                return;
            }

            var instance = new RelicInstance(so.Data, _context);
            _ownedRelics.Add(id, instance);
            instance.Activate();
            
            RelicAdded?.Invoke(instance);
            // TODO: Relic Bar에 아이콘 추가
        }

        private class RelicContext : IRelicContext
        {
            public PlayerStatus PlayerStatus { get; }
            public ScoreProviderRegistry ScoreProviderRegistry { get; }
            
            public RelicContext(
                PlayerStatus playerStatus,
                ScoreProviderRegistry scoreProviderRegistry)
            {
                PlayerStatus = playerStatus;
                ScoreProviderRegistry = scoreProviderRegistry;
            }
        }
    }

    public interface IRelicContext
    {
        PlayerStatus PlayerStatus { get; }
        ScoreProviderRegistry ScoreProviderRegistry { get; }
    }
}