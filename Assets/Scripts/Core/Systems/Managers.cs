using Cardevil.Gameplay.Items;
using UnityEngine;

namespace Cardevil.Core.Systems
{
    public class Managers : MonoBehaviour
    {
        //
        static Managers s_instance;
        static Managers Instance { get { Init(); return s_instance; } }


        UI_Manager _ui = new UI_Manager();
        ExecutionManager _execution = new ExecutionManager();
        ItemManager _item = new ItemManager();
    
        public static UI_Manager UI { get { return Instance._ui; } }
        public static ExecutionManager Execute { get { return Instance._execution; } }
        public static ItemManager Item { get { return Instance._item; } }

    
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
            }
        }

        public static void Clear()
        {
            UI.Clear();
        
        }
    }
}
