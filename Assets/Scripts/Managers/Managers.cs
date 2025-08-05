using Cardevil.Manager;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }


    UI_Manager _ui = new UI_Manager();
    ResourceManager _resource = new ResourceManager();
    GameManager _game = new GameManager();
    [SerializeField] PoolManager _pool = new PoolManager();
    SceneManagerEx _scene = new SceneManagerEx();
    DataManager _data = new DataManager(); //DataManager가 겹쳐서 추가
    JsonManager _json = new JsonManager();
    SoundManager _sound = new SoundManager();
    ExecutionManager _execution = new ExecutionManager();

    public static GameManager Game { get { return Instance._game; } }
    public static UI_Manager UI { get { return Instance._ui; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } } 
    public static JsonManager Json {  get { return Instance._json; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static ExecutionManager Execute { get { return Instance._execution; } }
    
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
            s_instance._data.Init();
            // s_instance._sound.Init();                !!!!!!!!주의 나중에 사운드 작업할때 반드시 켜야함.
		    s_instance._execution.Init();
        }
    }

    public static void Clear()
    {
        UI.Clear();
        Pool.Clear();
        Sound.Clear();
        Game.Clear();
    }

}
