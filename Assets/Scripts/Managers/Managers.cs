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
    [SerializeField] PoolManager _pool = new PoolManager();
    JsonManager _json = new JsonManager();
    ExecutionManager _execution = new ExecutionManager();
    [SerializeField] CardManager _card = new CardManager();
    ItemManager _item = new ItemManager();
    [SerializeField] RelicManager _relic = new RelicManager();
    [SerializeField] DungeonManager _dungeon = new DungeonManager();
    
    public static UI_Manager UI { get { return Instance._ui; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static JsonManager Json {  get { return Instance._json; } }
    public static ExecutionManager Execute { get { return Instance._execution; } }
    public static CardManager Card { get { return Instance._card; } }
    public static ItemManager Item { get { return Instance._item; } }
    public static RelicManager Relic {get { return Instance._relic; }}
    public static DungeonManager Dungeon { get { return Instance._dungeon; } }

    
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
            s_instance._pool.Init();
            s_instance._execution.Init();
            s_instance._card.Init();
            s_instance._dungeon.Init();
            s_instance._item.Init();
            s_instance._relic.Init();
        }
    }

    public static void Clear()
    {
        UI.Clear();
        Pool.Clear();
        
    }
    
    


}
