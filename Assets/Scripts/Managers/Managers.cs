using Cardevil.Manager;
using Cardevil.Pools;
using UnityEngine;
using Cardevil.Relics;

public class Managers : MonoBehaviour
{
    //
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }


    UI_Manager _ui = new UI_Manager();
    ExecutionManager _execution = new ExecutionManager();
    ItemManager _item = new ItemManager();
    [SerializeField] RelicManager _relic = new RelicManager();
    
    public static UI_Manager UI { get { return Instance._ui; } }
    public static ExecutionManager Execute { get { return Instance._execution; } }
    public static ItemManager Item { get { return Instance._item; } }
    public static RelicManager Relic {get { return Instance._relic; }}

    
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
            s_instance._execution.Init();
            s_instance._item.Init();
            s_instance._relic.Init();
        }
    }

    public static void Clear()
    {
        UI.Clear();
        
    }
}
