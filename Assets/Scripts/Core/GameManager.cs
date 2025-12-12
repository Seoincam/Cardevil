using Cardevil.Attributes;
using Cardevil.Cards.Data;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Ingame.Field;
using Cardevil.Save;
using Cardevil.Systems;
using Cardevil.Utils;
using Cardevil.Enemy;
using Cardevil.Ingame.Player;

namespace Cardevil.Core
{
    [System.Serializable]
    public class GameManager : ISaveLoad
    {
        [Header("State")] [SerializeField, VisibleOnly]
        private PlayerStatus playerStatus = new(); // 플레이어 상태 

        [SerializeField, VisibleOnly] private CardLibrary cardLibrary = new();

        [Header("References")] [SerializeField, VisibleOnly]
        private Field _field;

        [SerializeField, VisibleOnly] private PlayerCharacter _player; // 임시 플레이어'

        private TurnManager _turn = new();
        private EnemySpawner _enemySpawner;

        public Field Field
        {
            get
            {
                if (_field == null)
                {
                    _field = Object.FindAnyObjectByType<Field>();
                    if (_field == null)
                    {
                        Debug.LogError("Field not found in the scene.");
                    }
                }

                return _field;
            }
            set
            {
                _field = value;
            }
        }

        public ITurnEnemy Enemy => _turn.Enemy;

        public PlayerCharacter Player
        {
            get
            {
                return _player;
            }
            set
            {
                _player = value;
            }
        }

        public int TurnOrder => _turn.TurnOrder;

        public PlayerStatus PlayerStatus
        {
            get
            {
                return playerStatus;
            }
        }

        public void Init()
        {

            // 세이브에 등록
            // 각 요소를 직접 등록할 수 있지만,
            // 혹시 모르는 Reference 문제를 방지하기 위해 this로 등록 후 Save, Load에서 처리
            // SaveLoadManager saveLoadManager = Managers.SaveLoad;
            // saveLoadManager.Register(this);

            _enemySpawner = new EnemySpawner();
            _turn.Init(_enemySpawner);
        }

        public void Clear()
        {

        }

        /// <summary>
        /// 전투 스테이지에 진입합니다.
        /// </summary>
        /// <param name="stageId">Map에서 선택된 지점의 Id.</param>
        public void EnterStage(string stageId)
        {
            /*
             * TODO:
             * 0. 지금까지 데이터 Save
             * 1. 스테이지 외 UI 가리기
             * 2. 스테이지 초기화
             * 3. 스테이지 구성 (완)
             */


            // 3. 스테이지 구성
            _enemySpawner.ConfigStageMobData(stageId);
            if (!_enemySpawner.TrySpawn(out var enemy))
            {
                LogEx.LogError($"Failed to spawn Enemy. room Id: {stageId}");
                return;
            }

            _turn.Register(Managers.Card.BuildFlow(), _player, enemy);
            _turn.StartLoop();
        }

        //플레이어 죽을 때 실행시킬 함수
        public void PlayerDied()
        {
            if (PlayerStatus.canRevive)
            {
                //부활하기
                PlayerStatus.CurrentHp = 1;

            }
        }

        public void GameStart()
        {
            // Debug.Log("[GameManager] Database가 초기화 되었습니다. GameStart 실행.");
            LogEx.Log("Database가 초기화 되었습니다. GameStart 실행.");
            playerStatus = new PlayerStatus();
            playerStatus.BroadcastInitialStatus();
        }

        public void Save(GameSave currentSave)
        {
            playerStatus.Save(currentSave);
            cardLibrary.Save(currentSave);
        }

        // TODO: 로드할 거 없을 때 처리하기
        public void Load(GameSave currentSave)
        {
            playerStatus.Load(currentSave);
            cardLibrary.Load(currentSave);
        }
    }
}