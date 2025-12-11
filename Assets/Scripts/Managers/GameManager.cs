using Cardevil.Attributes;
using Cardevil.Ingame;
using UnityEngine;
using Cardevil.Ingame.Field;
using Cardevil.InGame.Enemy;
using Cardevil.Save;
using UnityEngine.Serialization;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Cardevil.Enemy;
using Cardevil.Ingame.Player;
using Database;

[System.Serializable]
public class GameManager : ISaveLoad
{
    [SerializeField, VisibleOnly] private Field _field;
    [SerializeField, VisibleOnly] private PlayerCharacter _player; // 임시 플레이어'
    [SerializeField, VisibleOnly] private PlayerStatus _playerStatus = new(); // 플레이어 상태 

    [SerializeField, VisibleOnly] private TurnManager _turn = new();
    [SerializeField, VisibleOnly] private EnemySpawner _enemySpawner;
    
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
            return _playerStatus;
        }
    }
    
    public void Init()
    {
        
        // 세이브에 등록
        // 각 요소를 직접 등록할 수 있지만,
        // 혹시 모르는 Reference 문제를 방지하기 위해 this로 등록 후 Save, Load에서 처리
        SaveLoadManager saveLoadManager = Managers.SaveLoad;
        saveLoadManager.Register(this);

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
            Managers.Game.PlayerStatus.CurrentHp = 1;

        }
    }
    
    public void GameStart()
    {
        if (DatabaseManager.Instance.IsInitialized == false)
        {
            // Debug.LogWarning("[GameManager] Database가 초기화 되지 않았으므로 GameStart를 실행할 수 없습니다.");
            // Debug.LogWarning("[GameManager] Database 초기화를 기다립니다...");
            LogEx.LogWarning("Database가 초기화 되지 않았으므로 GameStart를 실행할 수 없습니다.");
            LogEx.LogWarning("Database 초기화를 기다립니다...");
            LoadPlayerData().Forget();
            return;
        }
        // Debug.Log("[GameManager] Database가 초기화 되었습니다. GameStart 실행.");
        LogEx.Log("Database가 초기화 되었습니다. GameStart 실행.");
        _playerStatus = new PlayerStatus();
        _playerStatus.BroadcastInitialStatus();
    }
    
    public void Save(GameSave currentSave)
    {
        _playerStatus.Save(currentSave);
    }

    public void Load(GameSave currentSave)
    {
        _playerStatus.Load(currentSave);
        
    }
    
    private async UniTask LoadPlayerData()
    {
        await UniTask.WaitUntil(() => DatabaseManager.Instance.IsInitialized);
        GameStart();
    }
}
