using UnityEngine;

namespace Cardevil.UI
{
    public class GlobalSceneCanvas : MonoBehaviour
    {
        private static GlobalSceneCanvas _instance;

        public static GlobalSceneCanvas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GlobalSceneCanvas>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
        
    }
}