using Cardevil.Core.Bootstrap;
using Cardevil.Core.Turn.Interfaces;
using Cardevil.Utils;
using Database;
using UnityEngine;
using System.Collections.Generic;
using Database.Generated;

namespace Cardevil.Enemy
{
    public class EnemySpawner
    {
        // Key(string) = RoomID, Value = RoomData
        private readonly Dictionary<string, RoomData> _roomDataDict = new();

        // Key(string) = MobID, Value = BaseMobBossData
        private readonly Dictionary<string, BaseMobBossData> _mobBossDataDict = new();

        private readonly Queue<string> _currentStageMobIds = new();
        
        public EnemySpawner()
        {
            var db = Bootstrapper.Instance.Database.Database;
        
            var baseMobBossDataList = db.BaseMobBossDataList;
            var roomDataList = db.RoomDataList;

            // 딕셔너리 초기화 (중복 방지)
            _roomDataDict.Clear();
            foreach (RoomData room in roomDataList)
                _roomDataDict.TryAdd(room.RoomID, room);

            // 몹/보스 데이터로 딕셔너리 빌드
            _mobBossDataDict.Clear();
            foreach (BaseMobBossData mob in baseMobBossDataList)
                _mobBossDataDict.TryAdd(mob.MobID, mob);
        }
        
        /// <summary>
        /// 새로운 Stage로 진입할때 실행합니다.
        /// </summary>
        public void ConfigStageMobData(string stageId)
        {
            if (!_roomDataDict.TryGetValue(stageId, out RoomData roomData))
            {
                LogEx.LogError($"[EnemySpawner] 'roomID' ({stageId})에 해당하는 RoomData를 딕셔너리에서 찾을 수 없습니다.");
                return;
            }
            int settingEnemyCount = roomData.MobCount;
            if (settingEnemyCount <= 0)
            {
                LogEx.LogWarning($"enemy count: {settingEnemyCount}. stage id: {stageId}");
                return;
            }
            if (roomData.MobList == null || roomData.MobList.Count == 0)
            {
                LogEx.LogWarning($"[EnemySpawner] Room '{stageId}'의 MobList가 비어있습니다.");
                return;
            }

            _currentStageMobIds.Clear();
            
            // 몹의 수 에서 나올 수 있는 몬스터들의 id를 저장한다
            // 같은 몹이 여러 번 나올 수 있음.
            for (int i = 0; i < settingEnemyCount; i++)
            {
                int randomIndex = Random.Range(0, roomData.MobList.Count);
                string cleanMobID = roomData.MobList[randomIndex].Trim('"');
                
                _currentStageMobIds.Enqueue(cleanMobID);
            }
        }

        public bool TrySpawn(out ITurnEnemy spawned)
        {
            spawned = null;
            
            if (!_currentStageMobIds.TryDequeue(out string mobId))
                return false;
            
            spawned = SpawnEnemy(mobId);
            return true;
        }
        
        
        private InGame.Enemy.Enemy SpawnEnemy(string mobID)
        {
            // 딕셔너리에서 데이터를 찾기
            if (!_mobBossDataDict.TryGetValue(mobID, out BaseMobBossData dataToSpawn))
            {
                LogEx.LogError($"[EnemySpawner] MobID '{mobID}'에 해당하는 데이터를 딕셔너리에서 찾을 수 없습니다.");
                return null;
            }

            bool isBoss = false; // dataToSpawn.IsBoss;

            string prefabPath;
            if (isBoss)
            {
                // prefabPath = $"Prefabs/Enemy/Boss/{dataToSpawn.BossType}";
                prefabPath = $"Prefabs/Enemy/Enemy{dataToSpawn.MobID}"; // 임시
            }
            else
            {
                prefabPath = "Prefabs/Enemy/Enemy";
            }

            if (string.IsNullOrEmpty(prefabPath))
            {
                LogEx.LogError($"Prefab path가 없습니다.");
                return null;
            }

            // [권장] Resources.Load 대신 Addressables.LoadAssetAsync<GameObject>(prefabPath) 사용 고려
            GameObject enemyPrefab = Resources.Load<GameObject>(prefabPath);

            if (!enemyPrefab)
            {
                LogEx.LogError($"Enemy 프리팹 로드 실패: {prefabPath}");
                return null;
            }

            var go = Object.Instantiate(enemyPrefab);
            if (!go.TryGetComponent(out InGame.Enemy.Enemy enemy))
            {
                LogEx.LogError("스폰된 프리팹에 'Enemy' 컴포넌트가 없습니다.");
                return null;
            }
            enemy.Setup(dataToSpawn);
            
            return enemy;
        }
    }
}