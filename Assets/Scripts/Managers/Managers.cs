using Cardevil.Cards.System;
using Cardevil.Dungeon;
using Cardevil.Manager;
using Cardevil.Pools;
using Cardevil.Save;
using Cardevil.Sound;
using Cardevil.Systems;
using Database;
using UnityEngine;
using Cardevil.DataStructure;
using Cardevil.Relics;

public class Managers : MonoBehaviour
{
    //
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }


    UI_Manager _ui = new UI_Manager();
    ResourceManager _resource = new ResourceManager();
    GameManager _game = new GameManager();
    [SerializeField] PoolManager _pool = new PoolManager();
    SceneManagerEx _scene = new SceneManagerEx();
    [SerializeField]SaveLoadManager _saveload = new SaveLoadManager();
    JsonManager _json = new JsonManager();
    [SerializeField] SoundManager _sound = new SoundManager();
    ExecutionManager _execution = new ExecutionManager();
    EventManager _event = new EventManager();
    TurnManager _turn = new TurnManager();
    CardManager _card = new CardManager();
    ItemManager _item = new ItemManager();
    [SerializeField] RelicManager _relic = new RelicManager();
    [SerializeField] DungeonManager _dungeon = new DungeonManager();
    [SerializeField] DatabaseManager _database;
    public static GameManager Game { get { return Instance._game; } }
    public static UI_Manager UI { get { return Instance._ui; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static SaveLoadManager SaveLoad { get { return Instance._saveload; } } 
    public static JsonManager Json {  get { return Instance._json; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static ExecutionManager Execute { get { return Instance._execution; } }
    public static EventManager Event { get { return Instance._event; } }
    public static TurnManager Turn { get { return Instance._turn; } }
    public static CardManager Card { get { return Instance._card; } }
    public static ItemManager Item { get { return Instance._item; } }
    public static RelicManager Relic {get { return Instance._relic; }}
    public static DungeonManager Dungeon { get { return Instance._dungeon; } }
    public static DatabaseManager Database { get { return Instance._database; } }

    
    void Start()
    {
        Init();

    }
    // Update is called once per frame
    void Update()
    {
       
    }

    public static void Init()
    {

        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Manager");
            if (go == null)
            {

                go = new GameObject { name = "@Manager" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();
            s_instance._saveload.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();                //!!!!!!!!주의 나중에 사운드 작업할때 반드시 켜야함.
            s_instance._execution.Init();
            s_instance._card.Init();
            s_instance._dungeon.Init();
            s_instance._item.Init();
            // s_instance._relic.Init();
            s_instance._game.Init();

            if (s_instance._database == null)
            {
                s_instance._database = FindAnyObjectByType<DatabaseManager>();
                if (s_instance._database == null)
                {
                    GameObject obj = new GameObject("@DatabaseManager");
                    s_instance._database = obj.AddComponent<DatabaseManager>();
                }
            }
            
            s_instance._database.Initialize();
        }
    }

    public static void Clear()
    {
        UI.Clear();
        Sound.Clear();
        Pool.Clear();
        Game.Clear();
    }
    
    


}
