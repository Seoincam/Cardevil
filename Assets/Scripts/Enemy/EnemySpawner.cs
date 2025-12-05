using UnityEngine;
using System.Collections.Generic;
using Database;
using Database.Generated;
using Cardevil.Enemy; 
using Cardevil.InGame.Enemy;

namespace Cardevil.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        // Key(string) = RoomID, Value = RoomData
        private Dictionary<string, RoomData> roomDataDict = new Dictionary<string, RoomData>();

        // Key(string) = MobID, Value = BaseMobBossData
        private Dictionary<string, BaseMobBossData> mobBossDataDict = new Dictionary<string, BaseMobBossData>();



        [SerializeField] GameObject enemyContainer;
        void Start()
        {
            Managers.Game._enemySpawner = this;

            Init();
        }
        private bool isInit = false;
        // 딕셔너리 초기화 및 지정
        void Init()
        {
            if (!isInit)
            {
                List<BaseMobBossData> baseMobBossDataList = Managers.Game._database.Database.BaseMobBossDataList;
                List<RoomData> roomDataList = Managers.Game._database.Database.RoomDataList;

                // 딕셔너리 초기화 (중복 방지)
                roomDataDict.Clear();
                foreach (RoomData room in roomDataList)
                {
                    if (!roomDataDict.ContainsKey(room.RoomID))
                    {
                        roomDataDict.Add(room.RoomID, room);
                    }
                }

                // 몹/보스 데이터로 딕셔너리 빌드
                mobBossDataDict.Clear();
                foreach (BaseMobBossData mob in baseMobBossDataList)
                {
                    if (!mobBossDataDict.ContainsKey(mob.MobID))
                    {
                        mobBossDataDict.Add(mob.MobID, mob);
                    }
                }
                isInit = true;


                // 기믹에 해당하는 Prefab을 불러옵니다.
                // test
                InitiateRoomEnemy("Test");
            }
        }

        /// <summary>
        /// 새로운 Room으로 진입할때 실행합니다.
        /// </summary>
        public void InitiateRoomEnemy(string roomID)
        {
            // roomID와 같은 RoomData를 받기
            if (!roomDataDict.TryGetValue(roomID, out RoomData getRoomData))
            {
                Debug.LogError($"[EnemySpawner] 'roomID' ({roomID})에 해당하는 RoomData를 딕셔너리에서 찾을 수 없습니다.");
                return;
            }

            // 방별 몹의 수 를 받는다.
            int settingEnemyCount = getRoomData.MobCount;
            if (settingEnemyCount <= 0)
            {
                return; // 스폰할 몹 없음
            }

            List<string> possibleMobIDs = getRoomData.MobList;
            if (possibleMobIDs == null || possibleMobIDs.Count == 0)
            {
                Debug.LogError($"[EnemySpawner] Room '{roomID}'의 MobList가 비어있습니다.");
                return;
            }

            // 몹의 수 에서 나올 수 있는 몬스터들의 id를 저장한다
            List<string> settingEnemyTypeStringList = new List<string>();
            for (int i = 0; i < settingEnemyCount; i++)
            {
                int randomIndex = Random.Range(0, possibleMobIDs.Count);

                // 가져온 ID에서 양쪽 끝의 따옴표(")를 제거합니다.
                string cleanMobID = possibleMobIDs[randomIndex].Trim('"');

                // settingEnemyTypeStringList.Add(랜덤으로 저장);
                settingEnemyTypeStringList.Add(cleanMobID);
            }

            // 모든 몹을 instantiate한다.
            foreach (string mobIDToSpawn in settingEnemyTypeStringList)
            {
                // 인덱스가 아닌 MobID를 직접 넘깁니다.
                EnemyInstatntiate(mobIDToSpawn);
            }
        }

        /// <summary>
        /// mobID에 해당하는 몹을 Instantiate
        /// </summary>
        private void EnemyInstatntiate(string mobID)
        {
            // 딕셔너리에서 데이터를 찾기
            if (!mobBossDataDict.TryGetValue(mobID, out BaseMobBossData dataToSpawn))
            {
                Debug.LogError($"[EnemySpawner] MobID '{mobID}'에 해당하는 데이터를 딕셔너리에서 찾을 수 없습니다.");
                return;
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
                Debug.LogError($"Prefab path가 없습니다.");
                return;
            }

            // [권장] Resources.Load 대신 Addressables.LoadAssetAsync<GameObject>(prefabPath) 사용 고려
            GameObject enemyPrefab = Resources.Load<GameObject>(prefabPath);

            if (enemyPrefab == null)
            {
                Debug.LogError($"프리팹 로드 실패: {prefabPath}");
                return;
            }

            GameObject newEnemyObject = Instantiate(enemyPrefab, enemyContainer.transform);
            Cardevil.InGame.Enemy.Enemy enemyScript = newEnemyObject.GetComponent<Cardevil.InGame.Enemy.Enemy>();

            if (enemyScript != null)
            {
                enemyScript.Setup(dataToSpawn);
            }
            else
            {
                Debug.LogWarning($"스폰된 프리팹에 'Enemy' 컴포넌트가 없습니다.");
            }
        }


        #region QA


        #endregion

    }
}