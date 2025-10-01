using Cardevil.Ingame;
using System;
using System.Collections.Generic;
using UnityEngine;
using Cardevil.Ingame.Field;
using Cardevil.Ingame.Entities;
using Cardevil.InGame.Enemy;
using Cardevil.Save;
using UnityEngine.Serialization;
using Cardevil.Systems;
using Cardevil.Utils;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using Database;

[Serializable]
public class GameManager : ISaveLoad
{
    [FormerlySerializedAs("field")] [SerializeField] private Field _field;
    [FormerlySerializedAs("enemy")] [SerializeField] private Enemy _enemy;
    [FormerlySerializedAs("turnOrder")] public int _turnOrder = 0;
    [FormerlySerializedAs("entity")] [SerializeField] private PlayerCharacter _player; // 임시 플레이어'
    [SerializeField] private PlayerStatus _playerStatus = new PlayerStatus(); // 플레이어 상태 
    [SerializeField] public DatabaseManager _database;
    

    public Field Field
    {
        get
        {
            if (_field == null)
            {
                _field = GameObject.FindAnyObjectByType<Field>();
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

    public Enemy Enemy
    {
        get
        {
            return _enemy;
        }
        set
        {
            _enemy = value;
        }
    }
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
    
    public int TurnOrder
    {
        get { return _turnOrder; }
        set
        {
            if (value < 0)
            {
                Debug.LogError("TurnOrder cannot be negative.");
                return;
            }
            _turnOrder = value;
        }
    }
    
    public PlayerStatus PlayerStatus
    {
        get
        {
            return _playerStatus;
        }
    }
    
    
    
    public void Clear()
    {
 
     }

    //게임 상태를 나눠서 상태에 따라 스크립트들이 돌아가게 함
    public enum GameState
    {
        Pause,
        Combat,
        NonCombat
    }
    public GameState currentState;


    public void Init()
    {
        
        // 세이브에 등록
        // 각 요소를 직접 등록할 수 있지만,
        // 혹시 모르는 Reference 문제를 방지하기 위해 this로 등록 후 Save, Load에서 처리
        SaveLoadManager saveLoadManager = Managers.SaveLoad;
        saveLoadManager.Register(this);
        
    }
    
    //플레이어 죽을 때 실행시킬 함수
    public void PlayerDied()
    {

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
    
    private async UniTask LoadPlayerData()
    {
        await UniTask.WaitUntil(() => Managers.Database.IsInitialized);
        GameStart();
    }

    public void StageStart()
    {
        TurnOrder = 0;
        Managers.Relic.Init();
        Managers.Card.OnEnterStage();
        Managers.Turn.Init(
            Managers.Card.RerollInput,
            Managers.Card.PlayerInput,
            Player.GetComponent<ITurnPlayerMove>(),
            Player.GetComponent<ITurnPlayerAction>(),
            Enemy.GetComponent<ITurnEnemy>()
            );

        Managers.Turn.StartLoop();
    }

    public void Save(GameSave currentSave)
    {
        _playerStatus.Save(currentSave);
    }

    public void Load(GameSave currentSave)
    {
        _playerStatus.Load(currentSave);
        
    }
}
