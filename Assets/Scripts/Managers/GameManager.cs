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

[System.Serializable]
public class GameManager : ISaveLoad
{
    [FormerlySerializedAs("field")] [SerializeField] private Field _field;
    [FormerlySerializedAs("entity")] [SerializeField] private PlayerCharacter _player; // 임시 플레이어'
    [SerializeField] private PlayerStatus _playerStatus = new PlayerStatus(); // 플레이어 상태 

    private readonly TurnManager _turn = new();
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
    
    public void EnterStage(string stageId)
    {
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
        if (Managers.Database.IsInitialized == false)
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
        await UniTask.WaitUntil(() => Managers.Database.IsInitialized);
        GameStart();
    }
}
